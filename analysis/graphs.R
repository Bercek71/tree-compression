# ================================================================
# ANALÝZA KOMPRESE STROMOVÝCH STRUKTUR
# ================================================================

# Instalace a načtení potřebných knihoven
if (!require("showtext")) install.packages("showtext")
if (!require("ggplot2")) install.packages("ggplot2")
if (!require("dplyr")) install.packages("dplyr")
if (!require("tidyr")) install.packages("tidyr")
if (!require("scales")) install.packages("scales")
if (!require("patchwork")) install.packages("patchwork")
if (!require("viridis")) install.packages("viridis")
if (!require("readr")) install.packages("readr")
if (!require("gridExtra")) install.packages("gridExtra")

library(showtext)
library(ggplot2)
library(dplyr)
library(tidyr)
library(scales)
library(patchwork)
library(viridis)
library(readr)
library(gridExtra)

# Aktivace podpory českých znaků
showtext_auto()

# Nastavení kořene pro cesty pro relativní cesty
setwd(dirname(rstudioapi::getActiveDocumentContext()$path))

# ================================================================
# NAČTENÍ DAT
# ================================================================

# Načtení všech čtyř datasetů z reportů
report_original <- read_csv("../docs/data/reports/report - treerepair linear.csv")
report_optimized <- read_csv("../docs/data/reports/report - treerepair - linear n-arr.csv")
report_no_linear <- read_csv("../docs/data/reports/report - treerepair - no linearization.csv")
report_no_linear_opt <- read_csv("../docs/data/reports/report - treerepair - no linearization optimized.csv")

# Přejmenování datasetů pro lepší přehlednost - Upravené podle nové terminologie
names_mapping <- c(
  "report_original" = "Linearizace + RePair",
  "report_optimized" = "Opt. linearizace + RePair",
  "report_no_linear" = "Komprese bez linearizace",
  "report_no_linear_opt" = "Opt. komprese bez linearizace"
)

# ================================================================
# PŘÍPRAVA DAT
# ================================================================

# Funkce pro zpracování datového souboru
process_report <- function(report, name) {
  report %>%
    # Úprava názvů typů souborů pro lepší čitelnost
    mutate(Type = case_when(
      grepl("kernel_docs", Type) ~ "Technická dokumentace",
      grepl("prose", Type) ~ "Próza",
      grepl("legal_papers", Type) ~ "Právní dokumenty",
      grepl("research_papers", Type) ~ "Výzkumné články",
      TRUE ~ "Ostatní"
    )) %>%
    # odfiltrovat ostatní typy
    filter(Type != "Ostatní") %>%
    # Převod časů na milisekundy pro lepší interpretaci
    mutate(
      CompressionTime = as.numeric(CompressionTime) * 1000,
      DecompressionTime = as.numeric(DecompressionTime) * 1000,
      TextToTreeDuration = as.numeric(TextToTreeDuration) * 1000,
      # Výpočet efektivity komprese/dekomprese (ms / % změny velikosti)
      CompressionEfficiency = CompressionTime / (abs(1 - CompressionRatio) * 100),
      DecompressionEfficiency = DecompressionTime / (abs(1 - CompressionRatio) * 100),
      # Výpočet rychlosti komprese (B/ms)
      CompressionSpeed = Size / CompressionTime,
      DecompressionSpeed = CompressedSize / DecompressionTime,
      # Kompresní zisk v bytech
      CompressionGain = Size - CompressedSize,
      # Přidání názvu datasetu
      Dataset = name
    )
}

# Zpracování jednotlivých reportů
report_original_processed <- process_report(report_original, names_mapping["report_original"])
report_optimized_processed <- process_report(report_optimized, names_mapping["report_optimized"])
report_no_linear_processed <- process_report(report_no_linear, names_mapping["report_no_linear"])
report_no_linear_opt_processed <- process_report(report_no_linear_opt, names_mapping["report_no_linear_opt"])

# Spojení všech datasetů pro snadné porovnání
all_reports <- bind_rows(
  report_original_processed,
  report_optimized_processed,
  report_no_linear_processed,
  report_no_linear_opt_processed
)

# ================================================================
# DEFINICE SPOLEČNÉHO TÉMATU PRO GRAFY
# ================================================================

my_theme <- theme_minimal(base_size = 12) +
  theme(
    plot.title = element_text(face = "bold", size = 14, hjust = 0.5),
    plot.subtitle = element_text(size = 11, hjust = 0.5),
    axis.title = element_text(face = "bold"),
    legend.title = element_text(face = "bold"),
    panel.grid.minor = element_blank(),
    strip.text = element_text(face = "bold"),
    strip.background = element_rect(fill = "lightgray", color = NA)
  )

# Barevná paleta pro typy souborů
file_type_colors <- c(
  "Technická dokumentace" = "#2C7FB8", 
  "Próza" = "#7FBC41", 
  "Právní dokumenty" = "#D7301F",
  "Výzkumné články" = "#E6AB02",
  "Ostatní" = "#A6CEE3"
)

# Rozšířená barevná paleta pro metody komprese (4 metody) - Upravené podle nové terminologie
compression_method_colors <- c(
  "Linearizace + RePair" = "#1B9E77",
  "Opt. linearizace + RePair" = "#D95F02",
  "Komprese bez linearizace" = "#7570B3",
  "Opt. komprese bez linearizace" = "#E7298A"
)

# ================================================================
# GRAF 1: POROVNÁNÍ KOMPRESNÍHO POMĚRU
# ================================================================

compression_ratio_comparison <- ggplot(all_reports, aes(x = Type, y = CompressionRatio, fill = Dataset)) +
  geom_boxplot(alpha = 0.7, position = "dodge") +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  scale_fill_manual(values = compression_method_colors) +
  labs(
    title = "Porovnání kompresního poměru podle typu souboru a metody",
    subtitle = "Hodnoty pod čarou reprezentují úspěšnou kompresi (menší než originál)",
    x = "Typ souboru",
    y = "Kompresní poměr",
    fill = "Metoda komprese"
  ) +
  my_theme

# ================================================================
# GRAF 2: POROVNÁNÍ DOBY KOMPRESE
# ================================================================

compression_time_comparison <- ggplot(all_reports, aes(x = Type, y = CompressionTime, fill = Dataset)) +
  geom_boxplot(alpha = 0.7, position = "dodge") +
  scale_y_log10(labels = comma_format(suffix = " ms")) +
  scale_fill_manual(values = compression_method_colors) +
  labs(
    title = "Porovnání doby komprese podle typu souboru a metody",
    x = "Typ souboru",
    y = "Doba komprese (ms, logaritmická stupnice)",
    fill = "Metoda komprese"
  ) +
  my_theme

# ================================================================
# GRAF 3: POROVNÁNÍ DOBY DEKOMPRESE
# ================================================================

decompression_time_comparison <- ggplot(all_reports, aes(x = Type, y = DecompressionTime, fill = Dataset)) +
  geom_boxplot(alpha = 0.7, position = "dodge") +
  scale_y_log10(labels = comma_format(suffix = " ms")) +
  scale_fill_manual(values = compression_method_colors) +
  labs(
    title = "Porovnání doby dekomprese podle typu souboru a metody",
    x = "Typ souboru",
    y = "Doba dekomprese (ms, logaritmická stupnice)",
    fill = "Metoda komprese"
  ) +
  my_theme

# ================================================================
# GRAF 4: POMĚR DOBY KOMPRESE K DEKOMPRESI
# ================================================================

all_reports_ratio <- all_reports %>%
  mutate(CompressionToDecompressionRatio = CompressionTime / DecompressionTime)

comp_decomp_ratio <- ggplot(all_reports_ratio, aes(x = Type, y = CompressionToDecompressionRatio, fill = Dataset)) +
  geom_boxplot(alpha = 0.7, position = "dodge") +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  scale_y_log10(labels = comma_format()) +
  scale_fill_manual(values = compression_method_colors) +
  labs(
    title = "Poměr doby komprese k dekompresi",
    subtitle = "Hodnoty > 1 znamenají, že komprese trvá déle než dekomprese",
    x = "Typ souboru",
    y = "Poměr komprese/dekomprese (logaritmická stupnice)",
    fill = "Metoda komprese"
  ) +
  my_theme

# ================================================================
# GRAF 5: KOMPRESNÍ POMĚR PODLE VELIKOSTI SOUBORU
# ================================================================

compression_ratio_by_size <- ggplot(all_reports, aes(x = Size, y = CompressionRatio, color = Dataset)) +
  geom_point(size = 2, alpha = 0.7) +
  geom_smooth(method = "loess", se = TRUE, alpha = 0.2) +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  scale_x_log10(labels = comma_format()) +
  scale_color_manual(values = compression_method_colors) +
  facet_wrap(~Type, scales = "free") +
  labs(
    title = "Kompresní poměr podle velikosti souboru a typu",
    subtitle = "Rozděleno podle metody komprese",
    x = "Velikost souboru (B, logaritmická stupnice)",
    y = "Kompresní poměr",
    color = "Metoda komprese"
  ) +
  my_theme

# ================================================================
# GRAF 6: HISTOGRAM KOMPRESNÍHO POMĚRU
# ================================================================

compression_ratio_hist <- ggplot(all_reports, aes(x = CompressionRatio, fill = Dataset)) +
  geom_histogram(binwidth = 0.05, alpha = 0.7, position = "dodge") +
  scale_fill_manual(values = compression_method_colors) +
  labs(
    title = "Rozložení kompresního poměru podle metody komprese",
    x = "Kompresní poměr",
    y = "Počet souborů",
    fill = "Metoda komprese"
  ) +
  my_theme

# ================================================================
# GRAF 7: DOBA KOMPRESE VS VELIKOST SOUBORU
# ================================================================

compression_time_by_size <- ggplot(all_reports, aes(x = Size, y = CompressionTime, color = Dataset)) +
  geom_point(size = 2, alpha = 0.7) +
  geom_smooth(method = "lm", se = TRUE, alpha = 0.2) +
  scale_x_log10(labels = comma_format()) +
  scale_y_log10(labels = comma_format(suffix = " ms")) +
  scale_color_manual(values = compression_method_colors) +
  labs(
    title = "Doba komprese vs velikost souboru podle metody",
    x = "Velikost souboru (B, logaritmická stupnice)",
    y = "Doba komprese (ms, logaritmická stupnice)",
    color = "Metoda komprese"
  ) +
  my_theme

# ================================================================
# GRAF 8: KOMPRESNÍ ZISK VS DOBA KOMPRESE
# ================================================================

compression_gain_vs_time <- ggplot(all_reports, aes(x = CompressionTime, y = CompressionGain, color = Dataset)) +
  geom_point(size = 2, alpha = 0.7) +
  geom_smooth(method = "lm", se = TRUE, alpha = 0.2) +
  scale_x_log10(labels = comma_format(suffix = " ms")) +
  scale_y_log10(labels = comma_format(suffix = " B")) +
  scale_color_manual(values = compression_method_colors) +
  facet_wrap(~Type) +
  labs(
    title = "Kompresní zisk vs doba komprese podle typu souboru",
    x = "Doba komprese (ms, logaritmická stupnice)",
    y = "Kompresní zisk (B, logaritmická stupnice)",
    color = "Metoda komprese"
  ) +
  my_theme

# ================================================================
# GRAF 9: PRŮMĚRNÝ KOMPRESNÍ POMĚR PODLE TYPU A METODY
# ================================================================

average_compression_ratio <- all_reports %>%
  group_by(Type, Dataset) %>%
  summarise(
    AvgCompressionRatio = mean(CompressionRatio, na.rm = TRUE),
    .groups = "drop"
  )

avg_comp_ratio_plot <- ggplot(average_compression_ratio, aes(x = Type, y = AvgCompressionRatio, fill = Dataset)) +
  geom_bar(stat = "identity", position = "dodge", alpha = 0.8) +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  scale_fill_manual(values = compression_method_colors) +
  labs(
    title = "Průměrný kompresní poměr podle typu souboru a metody",
    x = "Typ souboru",
    y = "Průměrný kompresní poměr",
    fill = "Metoda komprese"
  ) +
  my_theme

# ================================================================
# GRAF 10: RYCHLOST KOMPRESE A DEKOMPRESE
# ================================================================

speed_data <- all_reports %>%
  pivot_longer(
    cols = c(CompressionSpeed, DecompressionSpeed),
    names_to = "SpeedType",
    values_to = "Speed"
  ) %>%
  mutate(
    SpeedType = case_when(
      SpeedType == "CompressionSpeed" ~ "Rychlost komprese",
      SpeedType == "DecompressionSpeed" ~ "Rychlost dekomprese"
    )
  )

speeds_plot <- ggplot(speed_data, aes(x = Dataset, y = Speed, fill = SpeedType)) +
  geom_boxplot(alpha = 0.7) +
  scale_y_log10(labels = comma_format(suffix = " B/ms")) +
  scale_fill_brewer(palette = "Set1") +
  facet_wrap(~Type, scales = "free") +
  labs(
    title = "Rychlost komprese a dekomprese podle typu souboru a metody",
    x = "Metoda komprese",
    y = "Rychlost (B/ms, logaritmická stupnice)",
    fill = "Operace"
  ) +
  my_theme +
  theme(axis.text.x = element_text(angle = 45, hjust = 1))

# ================================================================
# GRAF 11: EFEKTIVITA KOMPRESE A DEKOMPRESE
# ================================================================

efficiency_data <- all_reports %>%
  pivot_longer(
    cols = c(CompressionEfficiency, DecompressionEfficiency),
    names_to = "EfficiencyType",
    values_to = "Efficiency"
  ) %>%
  mutate(
    EfficiencyType = case_when(
      EfficiencyType == "CompressionEfficiency" ~ "Efektivita komprese",
      EfficiencyType == "DecompressionEfficiency" ~ "Efektivita dekomprese"
    )
  )

efficiency_plot <- ggplot(efficiency_data, aes(x = Dataset, y = Efficiency, fill = EfficiencyType)) +
  geom_boxplot(alpha = 0.7) +
  scale_y_log10(labels = comma_format()) +
  scale_fill_brewer(palette = "Set2") +
  facet_wrap(~Type, scales = "free") +
  labs(
    title = "Efektivita komprese a dekomprese podle typu souboru a metody",
    subtitle = "Menší hodnoty znamenají efektivnější kompresi/dekompresi",
    x = "Metoda komprese",
    y = "Efektivita (ms na změnu 1% velikosti, log)",
    fill = "Operace"
  ) +
  my_theme +
  theme(axis.text.x = element_text(angle = 45, hjust = 1))

# ================================================================
# GRAF 12: KOMPRESNÍ POMĚR PRO RŮZNÉ VELIKOSTI SOUBORŮ
# ================================================================

# Rozdělení do velikostních kategorií
all_reports <- all_reports %>%
  mutate(SizeCategory = case_when(
    Size < 1000 ~ "Malé (<1KB)",
    Size < 10000 ~ "Střední (1-10KB)",
    Size < 100000 ~ "Velké (10-100KB)",
    TRUE ~ "Velmi velké (>100KB)"
  )) %>%
  mutate(SizeCategory = factor(SizeCategory, levels = c("Malé (<1KB)", "Střední (1-10KB)", "Velké (10-100KB)", "Velmi velké (>100KB)")))

size_category_plot <- ggplot(all_reports, aes(x = SizeCategory, y = CompressionRatio, fill = Dataset)) +
  geom_boxplot(alpha = 0.7, position = "dodge") +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  scale_fill_manual(values = compression_method_colors) +
  labs(
    title = "Kompresní poměr podle velikostní kategorie souboru a metody",
    x = "Velikostní kategorie",
    y = "Kompresní poměr",
    fill = "Metoda komprese"
  ) +
  my_theme

# ================================================================
# GRAF 13: KOMPRESNÍ ZISK (%) PODLE TYPU SOUBORU
# ================================================================

all_reports <- all_reports %>%
  mutate(CompressionGainPercent = (Size - CompressedSize) / Size * 100)

gain_percent_plot <- ggplot(all_reports, aes(x = Type, y = CompressionGainPercent, fill = Dataset)) +
  geom_boxplot(alpha = 0.7, position = "dodge") +
  scale_fill_manual(values = compression_method_colors) +
  labs(
    title = "Procentuální kompresní zisk podle typu souboru a metody",
    subtitle = "Vyšší hodnoty znamenají lepší kompresi",
    x = "Typ souboru",
    y = "Kompresní zisk (%)",
    fill = "Metoda komprese"
  ) +
  my_theme

# ================================================================
# GRAF 14: DOBA ZPRACOVÁNÍ PODLE RŮZNÝCH FÁZÍ
# ================================================================

processing_times <- all_reports %>%
  pivot_longer(
    cols = c(TextToTreeDuration, CompressionTime, DecompressionTime),
    names_to = "Phase",
    values_to = "Duration"
  ) %>%
  mutate(
    Phase = case_when(
      Phase == "TextToTreeDuration" ~ "Převod textu na strom",
      Phase == "CompressionTime" ~ "Komprese",
      Phase == "DecompressionTime" ~ "Dekomprese"
    )
  )

phase_time_plot <- ggplot(processing_times, aes(x = Dataset, y = Duration, fill = Phase)) +
  geom_boxplot(alpha = 0.7) +
  scale_y_log10(labels = comma_format(suffix = " ms")) +
  scale_fill_brewer(palette = "Set3") +
  facet_wrap(~Type, scales = "free") +
  labs(
    title = "Trvání různých fází zpracování podle typu souboru a metody",
    x = "Metoda komprese",
    y = "Doba trvání (ms, logaritmická stupnice)",
    fill = "Fáze"
  ) +
  my_theme +
  theme(axis.text.x = element_text(angle = 45, hjust = 1))

# ================================================================
# GRAF 15: KOMPLEXNÍ POROVNÁNÍ METOD
# ================================================================

# Výpočet statistik pro všechny metody
stats_comparison <- all_reports %>%
  group_by(Type, Dataset) %>%
  summarise(
    AvgCompressionRatio = mean(CompressionRatio, na.rm = TRUE),
    MedianCompressionRatio = median(CompressionRatio, na.rm = TRUE),
    AvgCompressionTime = mean(CompressionTime, na.rm = TRUE),
    AvgDecompressionTime = mean(DecompressionTime, na.rm = TRUE),
    AvgCompressionGainPercent = mean(CompressionGainPercent, na.rm = TRUE),
    .groups = "drop"
  )

# Převod statistik do formátu "long" pro snadnější vizualizaci
stats_long <- stats_comparison %>%
  pivot_longer(
    cols = -c(Type, Dataset),
    names_to = "Metric",
    values_to = "Value"
  ) %>%
  mutate(
    Metric = case_when(
      Metric == "AvgCompressionRatio" ~ "Průměrný kompresní poměr",
      Metric == "MedianCompressionRatio" ~ "Mediánový kompresní poměr",
      Metric == "AvgCompressionTime" ~ "Průměrná doba komprese (ms)",
      Metric == "AvgDecompressionTime" ~ "Průměrná doba dekomprese (ms)",
      Metric == "AvgCompressionGainPercent" ~ "Průměrný kompresní zisk (%)"
    )
  )

# Rozdělení metrik do skupin pro lepší čitelnost grafů
metrics <- unique(stats_long$Metric)
ratio_metrics <- metrics[1:2]
time_metrics <- metrics[3:4]
gain_metrics <- metrics[5]

# Grafy pro jednotlivé skupiny metrik
ratio_comparison <- ggplot(
  filter(stats_long, Metric %in% ratio_metrics),
  aes(x = Type, y = Value, fill = Dataset)
) +
  geom_bar(stat = "identity", position = "dodge", alpha = 0.8) +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  facet_wrap(~Metric, scales = "free_y") +
  scale_fill_manual(values = compression_method_colors) +
  labs(
    title = "Porovnání kompresních poměrů podle metody a typu souboru",
    x = "Typ souboru",
    y = "Hodnota",
    fill = "Metoda komprese"
  ) +
  my_theme +
  theme(axis.text.x = element_text(angle = 45, hjust = 1))

time_comparison <- ggplot(
  filter(stats_long, Metric %in% time_metrics),
  aes(x = Type, y = Value, fill = Dataset)
) +
  geom_bar(stat = "identity", position = "dodge", alpha = 0.8) +
  facet_wrap(~Metric, scales = "free_y") +
  scale_y_log10(labels = comma_format()) +
  scale_fill_manual(values = compression_method_colors) +
  labs(
    title = "Porovnání doby zpracování podle metody a typu souboru",
    x = "Typ souboru",
    y = "Hodnota (logaritmická stupnice)",
    fill = "Metoda komprese"
  ) +
  my_theme +
  theme(axis.text.x = element_text(angle = 45, hjust = 1))

gain_comparison <- ggplot(
  filter(stats_long, Metric %in% gain_metrics),
  aes(x = Type, y = Value, fill = Dataset)
) +
  geom_bar(stat = "identity", position = "dodge", alpha = 0.8) +
  facet_wrap(~Metric, scales = "free_y") +
  scale_fill_manual(values = compression_method_colors) +
  labs(
    title = "Porovnání kompresního zisku podle metody a typu souboru",
    x = "Typ souboru",
    y = "Hodnota (%)",
    fill = "Metoda komprese"
  ) +
  my_theme +
  theme(axis.text.x = element_text(angle = 45, hjust = 1))

# ================================================================
# NOVÝ GRAF 16: POROVNÁNÍ LINEARIZOVANÝCH A NELINEARIZOVANÝCH METOD
# ================================================================

# Vytvoření nových kategorií pro skupinové porovnání
all_reports <- all_reports %>%
  mutate(
    MethodCategory = case_when(
      Dataset %in% c("Linearizace + RePair", "Opt. linearizace + RePair") ~ "S linearizací",
      Dataset %in% c("Komprese bez linearizace", "Opt. komprese bez linearizace") ~ "Bez linearizace"
    ),
    OptimizationCategory = case_when(
      Dataset %in% c("Linearizace + RePair", "Komprese bez linearizace") ~ "Základní",
      Dataset %in% c("Opt. linearizace + RePair", "Opt. komprese bez linearizace") ~ "Optimalizovaná"
    )
  )

# Graf porovnávající linearizované vs nelinearizované metody
linearization_comparison <- ggplot(all_reports, aes(x = Type, y = CompressionRatio, fill = MethodCategory)) +
  geom_boxplot(alpha = 0.7, position = "dodge") +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  scale_fill_manual(values = c("S linearizací" = "#1F78B4", "Bez linearizace" = "#33A02C")) +
  labs(
    title = "Vliv linearizace na kompresní poměr",
    subtitle = "Porovnání metod s linearizací a bez linearizace",
    x = "Typ souboru",
    y = "Kompresní poměr",
    fill = "Kategorie metody"
  ) +
  my_theme

# Graf porovnávající základní vs optimalizované metody
optimization_comparison <- ggplot(all_reports, aes(x = Type, y = CompressionRatio, fill = OptimizationCategory)) +
  geom_boxplot(alpha = 0.7, position = "dodge") +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  scale_fill_manual(values = c("Základní" = "#E31A1C", "Optimalizovaná" = "#FF7F00")) +
  labs(
    title = "Vliv optimalizace na kompresní poměr",
    subtitle = "Porovnání základních a optimalizovaných metod",
    x = "Typ souboru",
    y = "Kompresní poměr",
    fill = "Kategorie optimalizace"
  ) +
  my_theme

# ================================================================
# NOVÝ GRAF 17: POROVNÁNÍ VELIKOSTI VÝSTUPU VŮČI VSTUPNÍMU SOUBORU
# ================================================================

output_size_comparison <- ggplot(all_reports, aes(x = Size, y = CompressedSize, color = Dataset)) +
  geom_point(size = 2, alpha = 0.7) +
  geom_smooth(method = "lm", se = TRUE, alpha = 0.2) +
  geom_abline(intercept = 0, slope = 1, linetype = "dashed", color = "darkred", size = 1) +
  scale_x_log10(labels = comma_format()) +
  scale_y_log10(labels = comma_format()) +
  scale_color_manual(values = compression_method_colors) +
  facet_wrap(~Type, scales = "free") +
  labs(
    title = "Porovnání velikosti výstupu vůči vstupnímu souboru",
    subtitle = "Body pod čárou znamenají úspěšnou kompresi",
    x = "Velikost vstupního souboru (B, logaritmická stupnice)",
    y = "Velikost výstupu (B, logaritmická stupnice)",
    color = "Metoda komprese"
  ) +
  my_theme

# ================================================================
# UKLÁDÁNÍ GRAFŮ
# ================================================================

# Vytvoření adresáře pro grafy, pokud neexistuje
dir.create("visualization", showWarnings = FALSE)


# Funkce pro ukládání grafů
save_plot <- function(plot, filename, width = 12, height = 8) {
# save to pdf
  pdf(paste0("visualization/", filename, ".pdf"), width = width, height = height)
  print(plot)
  dev.off()
}


# Uložení jednotlivých grafů
save_plot(compression_ratio_comparison, "01_compression_ratio_comparison")
save_plot(compression_time_comparison, "02_compression_time_comparison")
save_plot(decompression_time_comparison, "03_decompression_time_comparison")
save_plot(comp_decomp_ratio, "04_compression_decompression_ratio")
save_plot(compression_ratio_by_size, "05_compression_ratio_by_size", width = 14, height = 10)
save_plot(compression_ratio_hist, "06_compression_ratio_histogram")
save_plot(compression_time_by_size, "07_compression_time_by_size")
save_plot(compression_gain_vs_time, "08_compression_gain_vs_time", width = 14, height = 10)
save_plot(avg_comp_ratio_plot, "09_average_compression_ratio")
save_plot(speeds_plot, "10_compression_decompression_speeds", width = 14, height = 10)
save_plot(efficiency_plot, "11_compression_decompression_efficiency", width = 14, height = 10)
save_plot(size_category_plot, "12_compression_ratio_by_size_category")
save_plot(gain_percent_plot, "13_compression_gain_percent")
save_plot(phase_time_plot, "14_processing_phases_time", width = 14, height = 10)
save_plot(ratio_comparison, "15a_ratio_metrics_comparison", width = 14, height = 8)
save_plot(time_comparison, "15b_time_metrics_comparison", width = 14, height = 8)
save_plot(gain_comparison, "15c_gain_metrics_comparison", width = 14, height = 8)
# Save new plots
save_plot(linearization_comparison, "16_linearization_comparison")
save_plot(optimization_comparison, "17_optimization_comparison")
save_plot(output_size_comparison, "18_output_size_comparison", width = 14, height = 10)

# ================================================================
# SOUHRNNÉ DASHBOARDY
# ================================================================

# Dashboard 1: Kompresní poměry
dashboard_compression_ratio <- (compression_ratio_comparison / avg_comp_ratio_plot) | 
                              (compression_ratio_hist / size_category_plot)

# Dashboard 2: Časové metriky
dashboard_timing <- (compression_time_comparison / decompression_time_comparison) |
                    (comp_decomp_ratio / phase_time_plot)

# Dashboard 3: Výkon podle velikosti
dashboard_performance <- (compression_ratio_by_size / compression_time_by_size) /
                         (compression_gain_vs_time / gain_percent_plot)

# Dashboard 4: Nový dashboard pro porovnání linearizace a optimalizace
dashboard_method_comparison <- (linearization_comparison / optimization_comparison) |
                              (output_size_comparison)

# Uložení dashboardů
save_plot(dashboard_compression_ratio, "dashboard_1_compression_ratio", width = 16, height = 12)
save_plot(dashboard_timing, "dashboard_2_timing", width = 16, height = 14)
save_plot(dashboard_performance, "dashboard_3_performance", width = 16, height = 16)
save_plot(dashboard_method_comparison, "dashboard_4_method_comparison", width = 16, height = 12)

# ================================================================
# STATISTICKÁ ANALÝZA
# ================================================================

# Základní statistické metriky pro jednotlivé metody
summary_stats <- all_reports %>%
  group_by(Dataset, Type) %>%
  summarise(
    PočetSouborů = n(),
    PrůměrnáVelikost = mean(Size),
    Min_CompRatio = min(CompressionRatio),
    Avg_CompRatio = mean(CompressionRatio),
    Median_CompRatio = median(CompressionRatio),
    Max_CompRatio = max(CompressionRatio),
    Avg_CompTime = mean(CompressionTime),
    Avg_DecompTime = mean(DecompressionTime),
    Avg_CompGain = mean(CompressionGainPercent),
    .groups = "drop"
  )

# Uložení statistik do CSV
write.csv(summary_stats, "visualization/summary_statistics.csv", row.names = FALSE)

# Identifikace nejlepších a nejhorších případů
best_compression <- all_reports %>%
  group_by(Dataset) %>%
  slice_min(order_by = CompressionRatio, n = 1) %>%
  select(Dataset, FileName, Type, Size, CompressionRatio, CompressionTime)

worst_compression <- all_reports %>%
  group_by(Dataset) %>%
  slice_max(order_by = CompressionRatio, n = 1) %>%
  select(Dataset, FileName, Type, Size, CompressionRatio, CompressionTime)

# Uložení nejlepších a nejhorších případů
write.csv(best_compression, "visualization/best_compression_cases.csv", row.names = FALSE)
write.csv(worst_compression, "visualization/worst_compression_cases.csv", row.names = FALSE)

# Souhrnná tabulka průměrných hodnot podle typu souboru a metody
avg_metrics_table <- all_reports %>%
  group_by(Dataset, Type) %>%
  summarise(
    Avg_Ratio = mean(CompressionRatio),
    Avg_CompTime = mean(CompressionTime),
    Avg_DecompTime = mean(DecompressionTime),
    Avg_GainPercent = mean(CompressionGainPercent),
    Count = n(),
    .groups = "drop"
  )

# Uložení tabulky metrik
write.csv(avg_metrics_table, "visualization/avg_metrics_table.csv", row.names = FALSE)

# ================================================================
# DODATEČNÉ ANALÝZY PRO POROVNÁNÍ LINEARIZOVANÝCH A NELINEARIZOVANÝCH METOD
# ================================================================

# Průměrný kompresní poměr podle kategorie metody
avg_by_method_category <- all_reports %>%
  group_by(MethodCategory, Type) %>%
  summarise(
    AvgCompressionRatio = mean(CompressionRatio),
    MedianCompressionRatio = median(CompressionRatio),
    AvgCompressionTime = mean(CompressionTime),
    AvgCompressionGainPercent = mean(CompressionGainPercent),
    Count = n(),
    .groups = "drop"
  )

# Průměrný kompresní poměr podle kategorie optimalizace
avg_by_optimization_category <- all_reports %>%
  group_by(OptimizationCategory, Type) %>%
  summarise(
    AvgCompressionRatio = mean(CompressionRatio),
    MedianCompressionRatio = median(CompressionRatio),
    AvgCompressionTime = mean(CompressionTime),
    AvgCompressionGainPercent = mean(CompressionGainPercent),
    Count = n(),
    .groups = "drop"
  )

# Uložení analyzovaných metrik
write.csv(avg_by_method_category, "visualization/avg_by_method_category.csv", row.names = FALSE)
write.csv(avg_by_optimization_category, "visualization/avg_by_optimization_category.csv", row.names = FALSE)

# ================================================================
# GRAFY PRO POROVNÁNÍ RŮZNÝCH METRIK MEZI KATEGORIEMI
# ================================================================

# Graf porovnávající průměrný kompresní poměr mezi kategoriemi metod
method_category_avg_ratio <- ggplot(avg_by_method_category, 
                                  aes(x = Type, y = AvgCompressionRatio, fill = MethodCategory)) +
  geom_bar(stat = "identity", position = "dodge", alpha = 0.8) +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  scale_fill_manual(values = c("S linearizací" = "#1F78B4", "Bez linearizace" = "#33A02C")) +
  labs(
    title = "Průměrný kompresní poměr podle kategorie linearizace",
    x = "Typ souboru",
    y = "Průměrný kompresní poměr",
    fill = "Kategorie metody"
  ) +
  my_theme

# Graf porovnávající průměrný kompresní poměr mezi kategoriemi optimalizace
opt_category_avg_ratio <- ggplot(avg_by_optimization_category, 
                               aes(x = Type, y = AvgCompressionRatio, fill = OptimizationCategory)) +
  geom_bar(stat = "identity", position = "dodge", alpha = 0.8) +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  scale_fill_manual(values = c("Základní" = "#E31A1C", "Optimalizovaná" = "#FF7F00")) +
  labs(
    title = "Průměrný kompresní poměr podle kategorie optimalizace",
    x = "Typ souboru",
    y = "Průměrný kompresní poměr",
    fill = "Kategorie optimalizace"
  ) +
  my_theme

# Uložení grafů pro kategoriální porovnání
save_plot(method_category_avg_ratio, "19_method_category_avg_ratio")
save_plot(opt_category_avg_ratio, "20_opt_category_avg_ratio")

# ================================================================
# GRAFY PRO PREZENTAČNÍ ÚČELY
# ================================================================

# Jednoduchý přehledový graf pro prezentace
presentation_overview <- ggplot(
  avg_metrics_table,
  aes(x = Dataset, y = Avg_Ratio, fill = Dataset)
) +
  geom_bar(stat = "identity", alpha = 0.9) +
  geom_text(aes(label = sprintf("%.2f", Avg_Ratio)), vjust = -0.5) +
  facet_wrap(~Type) +
  scale_fill_manual(values = compression_method_colors) +
  labs(
    title = "Srovnání kompresních poměrů stromových algoritmů",
    subtitle = "Průměrný kompresní poměr podle typu souboru a metody",
    x = "Metoda komprese",
    y = "Průměrný kompresní poměr"
  ) +
  theme_minimal() +
  theme(
    plot.title = element_text(face = "bold", size = 16, hjust = 0.5),
    plot.subtitle = element_text(hjust = 0.5, size = 12),
    axis.title = element_text(face = "bold", size = 12),
    axis.text.x = element_text(angle = 45, hjust = 1),
    strip.text = element_text(face = "bold", size = 12),
    legend.position = "none"
  )

save_plot(presentation_overview, "presentation_overview", width = 14, height = 10)

# ================================================================
# POROVNÁNÍ VÝKONU MEZI RŮZNÝMI VELIKOSTMI SOUBORŮ
# ================================================================

# Průměrný kompresní poměr podle kategorie velikosti
size_category_avg <- all_reports %>%
  group_by(SizeCategory, Dataset) %>%
  summarise(
    AvgCompressionRatio = mean(CompressionRatio),
    AvgCompressionTime = mean(CompressionTime),
    Count = n(),
    .groups = "drop"
  ) %>%
  filter(Count >= 3) # Pouze kategorie s dostatečným počtem vzorků

# Graf pro průměrný kompresní poměr podle velikostní kategorie
size_cat_avg_ratio <- ggplot(size_category_avg, 
                           aes(x = SizeCategory, y = AvgCompressionRatio, fill = Dataset)) +
  geom_bar(stat = "identity", position = "dodge", alpha = 0.8) +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred") +
  scale_fill_manual(values = compression_method_colors) +
  labs(
    title = "Průměrný kompresní poměr podle velikostní kategorie",
    x = "Velikostní kategorie",
    y = "Průměrný kompresní poměr",
    fill = "Metoda komprese"
  ) +
  my_theme +
  theme(axis.text.x = element_text(angle = 45, hjust = 1))

save_plot(size_cat_avg_ratio, "21_size_cat_avg_ratio")

# ================================================================
# ZÁVĚREČNÉ SHRNUTÍ
# ================================================================

# Vytvořit závěrečnou srovnávací tabulku pro všechny metody
final_comparison <- all_reports %>%
  group_by(Dataset) %>%
  summarise(
    PrůměrnýKompresníPoměr = mean(CompressionRatio),
    MedianKompresníhoPoměru = median(CompressionRatio),
    PrůměrnáDobaKomprese_ms = mean(CompressionTime),
    PrůměrnáDobaDekomprese_ms = mean(DecompressionTime),
    PrůměrnýKompresníZisk_procent = mean(CompressionGainPercent),
    PoměrÚspěšnéKomprese = sum(CompressionRatio < 1) / n() * 100,
    PočetSouborů = n(),
    .groups = "drop"
  ) %>%
  arrange(PrůměrnýKompresníPoměr)

# Uložení závěrečné srovnávací tabulky
write.csv(final_comparison, "visualization/final_comparison.csv", row.names = FALSE)

# Závěrečné shrnutí
cat("\nAnalýza dokončena. Všechny grafy byly uloženy do adresáře 'visualization'.\n")
cat("\nSrovnání metod komprese:\n")
print(final_comparison)

# Vytvoření závěrečného srovnávacího grafu
final_comparison_long <- final_comparison %>%
  select(Dataset, PrůměrnýKompresníPoměr, PrůměrnáDobaKomprese_ms, PrůměrnýKompresníZisk_procent, PoměrÚspěšnéKomprese) %>%
  pivot_longer(
    cols = -Dataset,
    names_to = "Metric",
    values_to = "Value"
  )

# Normalizace hodnot pro snadné srovnání
final_comparison_normalized <- final_comparison_long %>%
  group_by(Metric) %>%
  mutate(
    NormalizedValue = case_when(
      Metric == "PrůměrnýKompresníPoměr" ~ 1 - (Value - min(Value)) / (max(Value) - min(Value)), # Nižší je lepší
      Metric == "PrůměrnáDobaKomprese_ms" ~ 1 - (Value - min(Value)) / (max(Value) - min(Value)), # Nižší je lepší
      Metric == "PrůměrnýKompresníZisk_procent" ~ (Value - min(Value)) / (max(Value) - min(Value)), # Vyšší je lepší
      Metric == "PoměrÚspěšnéKomprese" ~ (Value - min(Value)) / (max(Value) - min(Value)) # Vyšší je lepší
    )
  ) %>%
  ungroup()

# Závěrečný radar chart pro porovnání metod
# Příprava dat pro radar chart
radar_data <- final_comparison_normalized %>%
  pivot_wider(
    names_from = Metric,
    values_from = NormalizedValue
  )

# Uložení dat pro radar chart
write.csv(radar_data, "visualization/radar_data.csv", row.names = FALSE)

# Výpis informace o dokončení
cat("\nAnalýza komprese stromových struktur dokončena.\n")
cat("Počet analyzovaných souborů:", nrow(all_reports), "\n")
cat("Počet zpracovaných metod komprese:", length(unique(all_reports$Dataset)), "\n")
cat("Všechny grafy a analýzy byly uloženy do adresáře 'visualization'.\n")
