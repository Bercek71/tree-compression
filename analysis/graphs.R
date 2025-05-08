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
      grepl("prose", Type) ~ "Beletrie",
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
  "Beletrie" = "#7FBC41", 
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


# ================================================================
# DALŠÍ DOPLŇUJÍCÍ GRAFY K ANALÝZE KOMPRESE STROMOVÝCH STRUKTUR
# ================================================================

# ================================================================
# POKROČILÉ VIZUALIZACE
# ================================================================

# B1: Analýza stability kompresního poměru - směrodatná odchylka podle metody
stability_analysis <- all_reports %>%
  group_by(Dataset, Type) %>%
  summarise(
    MeanCompRatio = mean(CompressionRatio),
    StdDevCompRatio = sd(CompressionRatio),
    RangeCompRatio = max(CompressionRatio) - min(CompressionRatio),
    MedianCompRatio = median(CompressionRatio),
    IQR_CompRatio = IQR(CompressionRatio),
    .groups = "drop"
  )

stability_plot <- ggplot(stability_analysis, aes(x = Dataset, y = StdDevCompRatio, fill = Type)) +
  geom_bar(stat = "identity", position = "dodge", alpha = 0.8) +
  scale_fill_manual(values = file_type_colors) +
  labs(
    title = "Stabilita kompresního poměru podle metody a typu souboru",
    subtitle = "Nižší směrodatná odchylka znamená stabilnější výsledky komprese",
    x = "Metoda komprese",
    y = "Směrodatná odchylka kompresního poměru",
    fill = "Typ souboru"
  ) +
  my_theme +
  theme(axis.text.x = element_text(angle = 45, hjust = 1))

# B2: Jitter plot pro detailní zobrazení rozložení kompresních poměrů
jitter_plot <- ggplot(all_reports, aes(x = Type, y = CompressionRatio, color = Dataset)) +
  geom_jitter(position = position_jitterdodge(jitter.width = 0.2, dodge.width = 0.8), 
             alpha = 0.7, size = 2) +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  scale_color_manual(values = compression_method_colors) +
  labs(
    title = "Rozložení jednotlivých kompresních poměrů podle typu souboru a metody",
    subtitle = "Každý bod představuje jeden soubor",
    x = "Typ souboru",
    y = "Kompresní poměr",
    color = "Metoda komprese"
  ) +
  my_theme +
  theme(legend.position = "top")

# B3: Kombinovaný graf kompresního poměru (box + violin + jitter)
combined_ratio_plot <- ggplot(all_reports, aes(x = Type, y = CompressionRatio, fill = Dataset)) +
  geom_violin(alpha = 0.3, draw_quantiles = c(0.5), scale = "width", trim = TRUE) +
  geom_boxplot(width = 0.1, alpha = 0.8, outlier.shape = NA) +
  geom_jitter(position = position_jitterdodge(jitter.width = 0.1, dodge.width = 0.8), 
             alpha = 0.5, size = 1, aes(color = Dataset)) +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  scale_fill_manual(values = compression_method_colors) +
  scale_color_manual(values = compression_method_colors) +
  labs(
    title = "Komplexní vizualizace kompresního poměru",
    subtitle = "Kombinace violin plot, box plot a bodů pro jednotlivé soubory",
    x = "Typ souboru",
    y = "Kompresní poměr",
    fill = "Metoda komprese",
    color = "Metoda komprese"
  ) +
  my_theme +
  guides(color = "none")  # Skrýt duplicitní legendu pro barvu

# B4: Porovnání velikosti stromu napříč typy dokumentů
tree_size_comparison <- ggplot(all_reports, aes(x = Type, y = TreeSize, fill = Dataset)) +
  geom_boxplot(alpha = 0.7, position = "dodge") +
  scale_y_log10(labels = comma_format(suffix = " B")) +
  scale_fill_manual(values = compression_method_colors) +
  labs(
    title = "Porovnání velikosti stromu podle typu souboru a metody",
    x = "Typ souboru",
    y = "Velikost stromu (B, logaritmická stupnice)",
    fill = "Metoda komprese"
  ) +
  my_theme

# B5: Vizualizace kompresního poměru a doby zpracování jako dva grafy vedle sebe
side_by_side <- all_reports %>%
  pivot_longer(
    cols = c(CompressionRatio, TotalProcessingTime),
    names_to = "Metric",
    values_to = "Value"
  ) %>%
  mutate(
    Metric = case_when(
      Metric == "CompressionRatio" ~ "Kompresní poměr",
      Metric == "TotalProcessingTime" ~ "Celková doba zpracování (ms)"
    )
  )

side_by_side_plot <- ggplot(side_by_side, aes(x = Type, y = Value, fill = Dataset)) +
  geom_boxplot(alpha = 0.7, position = "dodge") +
  facet_wrap(~ Metric, scales = "free_y") +
  scale_fill_manual(values = compression_method_colors) +
  scale_y_continuous(
    sec.axis = sec_axis(~ ., name = "Celková doba zpracování (ms)"),
    labels = function(x) format(x, scientific = FALSE)
  ) +
  labs(
    title = "Porovnání kompresního poměru a celkové doby zpracování",
    x = "Typ souboru",
    y = "Hodnota",
    fill = "Metoda komprese"
  ) +
  my_theme

# B6: Rozložení kompresního poměru pro různé metody (více detailní histogramy)
detailed_histograms <- ggplot(all_reports, aes(x = CompressionRatio, fill = Dataset)) +
  geom_histogram(binwidth = 0.025, alpha = 0.7, position = "identity") +
  facet_grid(Dataset ~ Type, scales = "free_y") +
  scale_fill_manual(values = compression_method_colors) +
  geom_vline(xintercept = 1.0, linetype = "dashed", color = "darkred", size = 0.8) +
  labs(
    title = "Detailní histogramy kompresního poměru",
    subtitle = "Rozděleno podle metody komprese a typu souboru",
    x = "Kompresní poměr",
    y = "Počet souborů",
    fill = "Metoda komprese"
  ) +
  my_theme +
  theme(legend.position = "none")

# B7: Porovnání vlivu velikosti vs. typu souboru na kompresní poměr
size_vs_type_effect <- ggplot(all_reports, 
                             aes(x = Type, y = CompressionRatio, fill = SizeCategory)) +
  geom_boxplot(alpha = 0.7, position = "dodge") +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  scale_fill_viridis_d(option = "D", end = 0.8) +
  facet_wrap(~ Dataset) +
  labs(
    title = "Vliv typu souboru a velikostní kategorie na kompresní poměr",
    subtitle = "Rozděleno podle metody komprese",
    x = "Typ souboru",
    y = "Kompresní poměr",
    fill = "Velikostní kategorie"
  ) +
  my_theme +
  theme(axis.text.x = element_text(angle = 45, hjust = 1))

# B8: Analýza vlivu struktury stromu na kompresi - počet uzlů vs. kompresní poměr
tree_structure_influence <- ggplot(all_reports, 
                                 aes(x = TreeNodesCount, y = CompressionRatio, color = Dataset)) +
  geom_point(alpha = 0.7) +
  geom_smooth(method = "loess", se = TRUE, alpha = 0.2) +
  scale_x_log10(labels = comma_format()) +
  scale_color_manual(values = compression_method_colors) +
  facet_wrap(~ Type, scales = "free") +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  labs(
    title = "Vliv počtu uzlů stromu na kompresní poměr",
    x = "Počet uzlů stromu (logaritmická stupnice)",
    y = "Kompresní poměr",
    color = "Metoda komprese"
  ) +
  my_theme

# B9: Kompresní efektivita vzhledem k průměrné velikosti uzlu
all_reports <- all_reports %>%
  mutate(AvgNodeSize = ifelse(is.na(TreeNodesCount) | TreeNodesCount == 0, NA, TreeSize / TreeNodesCount))

node_size_effectiveness <- ggplot(
  all_reports %>% filter(!is.na(AvgNodeSize)), 
  aes(x = AvgNodeSize, y = CompressionRatio, color = Dataset)
) +
  geom_point(alpha = 0.7) +
  geom_smooth(method = "loess", se = TRUE, alpha = 0.2) +
  scale_x_log10(labels = comma_format(suffix = " B/uzel")) +
  scale_color_manual(values = compression_method_colors) +
  facet_wrap(~ Type, scales = "free") +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  labs(
    title = "Vliv průměrné velikosti uzlu na kompresní poměr",
    x = "Průměrná velikost uzlu (B/uzel, logaritmická stupnice)",
    y = "Kompresní poměr",
    color = "Metoda komprese"
  ) +
  my_theme

# B10: Trade-off mezi kompresním poměrem a rychlostí komprese
tradeoff_plot <- ggplot(all_reports, 
                      aes(x = CompressionRatio, y = CompressionTime, color = Dataset)) +
  geom_point(aes(size = Size), alpha = 0.7) +
  scale_y_log10(labels = comma_format(suffix = " ms")) +
  scale_color_manual(values = compression_method_colors) +
  scale_size_continuous(trans = "log10", range = c(2, 8), labels = comma_format()) +
  geom_vline(xintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  facet_wrap(~ Type, scales = "free_y") +
  labs(
    title = "Trade-off mezi kompresním poměrem a dobou komprese",
    subtitle = "Velikost bodů odpovídá velikosti souboru",
    x = "Kompresní poměr",
    y = "Doba komprese (ms, logaritmická stupnice)",
    color = "Metoda komprese",
    size = "Velikost souboru (B)"
  ) +
  my_theme

# B11: Porovnání kompresního poměru a poměru velikosti stromu k původní velikosti
ratio_comparison_plot <- ggplot(all_reports, 
                              aes(x = TreeToTextRatio, y = CompressionRatio, color = Dataset)) +
  geom_point(alpha = 0.7) +
  geom_smooth(method = "lm", se = TRUE, alpha = 0.2) +
  scale_x_log10(labels = comma_format()) +
  scale_color_manual(values = compression_method_colors) +
  facet_wrap(~ Type, scales = "free") +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  labs(
    title = "Vztah mezi poměrem velikosti stromu k textu a kompresním poměrem",
    x = "Poměr velikosti stromu / text (logaritmická stupnice)",
    y = "Kompresní poměr",
    color = "Metoda komprese"
  ) +
  my_theme

# B12: Porovnání efektivity pro malé, střední a velké soubory
size_category_box <- ggplot(all_reports, 
                          aes(x = SizeCategory, y = CompressionRatio, fill = Dataset)) +
  geom_boxplot(alpha = 0.7, position = "dodge") +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  scale_fill_manual(values = compression_method_colors) +
  facet_wrap(~ Type) +
  labs(
    title = "Kompresní poměr podle velikostní kategorie a typu souboru",
    subtitle = "Rozděleno podle metody komprese",
    x = "Velikostní kategorie",
    y = "Kompresní poměr",
    fill = "Metoda komprese"
  ) +
  my_theme +
  theme(axis.text.x = element_text(angle = 45, hjust = 1))

# B13: Spider (radar) chart pro komplexní porovnání metod
# Připravit data pro spider chart - průměrné hodnoty po normalizaci
radar_data_prep <- all_reports %>%
  group_by(Dataset) %>%
  summarise(
    AvgCompRatio = mean(CompressionRatio),
    AvgCompTime = mean(CompressionTime),
    AvgDecompTime = mean(DecompressionTime),
    AvgGainPercent = mean(CompressionGainPercent),
    AvgPerformance = mean(CompressionPerformance),
    .groups = "drop"
  )

# Normalizace hodnot pro spider chart (0-1 škála)
radar_data_normalized <- radar_data_prep %>%
  mutate(
    CompRatio_Norm = 1 - ((AvgCompRatio - min(AvgCompRatio)) / (max(AvgCompRatio) - min(AvgCompRatio))),
    CompTime_Norm = 1 - ((AvgCompTime - min(AvgCompTime)) / (max(AvgCompTime) - min(AvgCompTime))),
    DecompTime_Norm = 1 - ((AvgDecompTime - min(AvgDecompTime)) / (max(AvgDecompTime) - min(AvgDecompTime))),
    GainPercent_Norm = (AvgGainPercent - min(AvgGainPercent)) / (max(AvgGainPercent) - min(AvgGainPercent)),
    Performance_Norm = (AvgPerformance - min(AvgPerformance)) / (max(AvgPerformance) - min(AvgPerformance))
  ) %>%
  select(Dataset, ends_with("_Norm")) %>%
  pivot_longer(
    cols = -Dataset,
    names_to = "Metric",
    values_to = "Value"
  ) %>%
  mutate(
    Metric = case_when(
      Metric == "CompRatio_Norm" ~ "Kompresní poměr",
      Metric == "CompTime_Norm" ~ "Doba komprese",
      Metric == "DecompTime_Norm" ~ "Doba dekomprese",
      Metric == "GainPercent_Norm" ~ "Kompresní zisk",
      Metric == "Performance_Norm" ~ "Výkon komprese"
    )
  )

# Spider chart pro porovnání metod
spider_chart <- ggplot(radar_data_normalized, 
                      aes(x = Metric, y = Value, color = Dataset, group = Dataset)) +
  geom_polygon(aes(fill = Dataset), alpha = 0.1) +
  geom_line(size = 1) +
  geom_point(size = 3) +
  scale_color_manual(values = compression_method_colors) +
  scale_fill_manual(values = compression_method_colors) +
  coord_polar() +
  labs(
    title = "Porovnání kompresních metod - Spider Chart",
    subtitle = "Vyšší hodnoty jsou lepší pro všechny metriky (hodnoty normalizovány)",
    color = "Metoda komprese",
    fill = "Metoda komprese"
  ) +
  my_theme +
  theme(
    axis.text.y = element_blank(),
    axis.ticks.y = element_blank(),
    panel.grid.major.y = element_line(color = "gray90"),
    panel.grid.minor.y = element_blank()
  )

# B14: Analýza optimality - křivka Pareto fronty pro čas komprese vs. kompresní poměr
# Vytvoření jednoduchého scatter plotu
pareto_data <- all_reports %>%
  select(FileName, Dataset, Type, Size, CompressionRatio, CompressionTime)

# Funkce pro identifikaci Pareto optimálních bodů
identify_pareto <- function(df) {
  df_sorted <- df %>%
    arrange(CompressionRatio, CompressionTime)
  
  pareto_points <- c(1)
  current_min_time <- df_sorted$CompressionTime[1]
  
  for (i in 2:nrow(df_sorted)) {
    if (df_sorted$CompressionTime[i] < current_min_time) {
      pareto_points <- c(pareto_points, i)
      current_min_time <- df_sorted$CompressionTime[i]
    }
  }
  
  df_sorted$IsParetoOptimal <- FALSE
  df_sorted$IsParetoOptimal[pareto_points] <- TRUE
  
  return(df_sorted)
}

# Aplikace Pareto analýzy pro každý typ souboru
pareto_results <- list()
for (type in unique(pareto_data$Type)) {
  type_data <- pareto_data %>% filter(Type == type)
  pareto_results[[type]] <- identify_pareto(type_data)
}

pareto_all <- bind_rows(pareto_results)

# Vytvoření Pareto grafu
pareto_plot <- ggplot(pareto_all, 
                     aes(x = CompressionRatio, y = CompressionTime, color = Dataset)) +
  geom_point(aes(size = Size, shape = IsParetoOptimal), alpha = 0.7) +
  scale_y_log10(labels = comma_format(suffix = " ms")) +
  scale_color_manual(values = compression_method_colors) +
  scale_shape_manual(values = c(16, 17)) +
  scale_size_continuous(trans = "log10", range = c(2, 8), labels = comma_format()) +
  facet_wrap(~ Type, scales = "free") +
  labs(
    title = "Pareto optimální řešení: Kompresní poměr vs. Doba komprese",
    subtitle = "Trojúhelníky označují Pareto optimální body",
    x = "Kompresní poměr",
    y = "Doba komprese (ms, logaritmická stupnice)",
    color = "Metoda komprese",
    size = "Velikost souboru (B)",
    shape = "Pareto optimální"
  ) +
  my_theme

# B15: Heatmapa průměrného kompresního poměru podle typu souboru a metody
heatmap_comp_ratio <- all_reports %>%
  group_by(Dataset, Type) %>%
  summarise(
    AvgCompRatio = mean(CompressionRatio),
    .groups = "drop"
  )

heatmap_plot <- ggplot(heatmap_comp_ratio, aes(x = Type, y = Dataset, fill = AvgCompRatio)) +
  geom_tile(color = "white") +
  scale_fill_gradient2(low = "blue", mid = "white", high = "red", 
                     midpoint = 1, limits = c(min(heatmap_comp_ratio$AvgCompRatio), 
                                            max(heatmap_comp_ratio$AvgCompRatio))) +
  geom_text(aes(label = sprintf("%.2f", AvgCompRatio)), color = "black", size = 4) +
  labs(
    title = "Heatmapa průměrných kompresních poměrů",
    subtitle = "Podle typu souboru a metody komprese",
    x = "Typ souboru",
    y = "Metoda komprese",
    fill = "Průměrný\nkompresní poměr"
  ) +
  my_theme +
  theme(
    axis.text.x = element_text(angle = 45, hjust = 1),
    panel.grid.major = element_blank(),
    panel.grid.minor = element_blank()
  )

# B16: Porovnání distribuční funkce CDF kompresního poměru
cdf_plot <- ggplot(all_reports, aes(x = CompressionRatio, color = Dataset)) +
  stat_ecdf(geom = "step", pad = FALSE, size = 1) +
  geom_vline(xintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  scale_color_manual(values = compression_method_colors) +
  facet_wrap(~ Type, scales = "free") +
  labs(
    title = "Kumulativní distribuční funkce (CDF) kompresního poměru",
    subtitle = "Vyšší křivka u hodnoty 1.0 značí lepší metodu (více souborů pod hranicí úspěšné komprese)",
    x = "Kompresní poměr",
    y = "Kumulativní relativní četnost",
    color = "Metoda komprese"
  ) +
  my_theme

# B17: Porovnání relativního zlepšení optimalizovaných metod
# Vypočítat průměrné hodnoty pro základní a optimalizované metody
improvement_data <- all_reports %>%
  mutate(
    OptimizationPair = case_when(
      Dataset == "Linearizace + RePair" ~ "LinearizacePair",
      Dataset == "Opt. linearizace + RePair" ~ "LinearizacePair",
      Dataset == "Komprese bez linearizace" ~ "BezLinearizacePair",
      Dataset == "Opt. komprese bez linearizace" ~ "BezLinearizacePair"
    ),
    IsOptimized = case_when(
      Dataset %in% c("Opt. linearizace + RePair", "Opt. komprese bez linearizace") ~ "Optimalizovaná",
      TRUE ~ "Základní"
    )
  ) %>%
  group_by(Type, OptimizationPair, IsOptimized) %>%
  summarise(
    AvgCompRatio = mean(CompressionRatio),
    AvgCompTime = mean(CompressionTime),
    AvgGainPercent = mean(CompressionGainPercent),
    .groups = "drop"
  ) %>%
  pivot_wider(
    names_from = IsOptimized,
    values_from = c(AvgCompRatio, AvgCompTime, AvgGainPercent)
  ) %>%
  mutate(
    CompRatio_Improvement = (AvgCompRatio_Základní - AvgCompRatio_Optimalizovaná) / AvgCompRatio_Základní * 100,
    CompTime_Improvement = (AvgCompTime_Základní - AvgCompTime_Optimalizovaná) / AvgCompTime_Základní * 100,
    GainPercent_Improvement = (AvgGainPercent_Optimalizovaná - AvgGainPercent_Základní) / AvgGainPercent_Základní * 100
  )

# Převod do dlouhého formátu pro grafické znázornění
improvement_long <- improvement_data %>%
  select(Type, OptimizationPair, ends_with("_Improvement")) %>%
  pivot_longer(
    cols = ends_with("_Improvement"),
    names_to = "Metric",
    values_to = "ImprovementPercent"
  ) %>%
  mutate(
    OptimizationPair = case_when(
      OptimizationPair == "LinearizacePair" ~ "Linearizace + RePair",
      OptimizationPair == "BezLinearizacePair" ~ "Komprese bez linearizace"
    ),
    Metric = case_when(
      Metric == "CompRatio_Improvement" ~ "Zlepšení kompresního poměru",
      Metric == "CompTime_Improvement" ~ "Zlepšení doby komprese",
      Metric == "GainPercent_Improvement" ~ "Zlepšení kompresního zisku"
    )
  )

# Graf zlepšení
improvement_plot <- ggplot(improvement_long, 
                         aes(x = Type, y = ImprovementPercent, fill = OptimizationPair)) +
  geom_bar(stat = "identity", position = "dodge", alpha = 0.8) +
  geom_text(aes(label = sprintf("%.1f%%", ImprovementPercent)), 
           position = position_dodge(width = 0.9), vjust = -0.3, size = 3) +
  facet_wrap(~ Metric, scales = "free_y") +
  scale_fill_manual(values = c("Linearizace + RePair" = "#1F78B4", 
                              "Komprese bez linearizace" = "#33A02C")) +
  labs(
    title = "Relativní zlepšení optimalizovaných metod oproti základním",
    subtitle = "Kladné hodnoty znamenají zlepšení, záporné hodnoty zhoršení",
    x = "Typ souboru",
    y = "Relativní zlepšení (%)",
    fill = "Skupina metod"
  ) +
  my_theme +
  theme(axis.text.x = element_text(angle = 45, hjust = 1))

# B18: Porovnání "trade-off" mezi prostorovou a časovou složitostí
tradeoff_space_time <- ggplot(all_reports, 
                            aes(x = CompressionGainPercent, y = TotalProcessingTime / Size * 1000, 
                               color = Dataset)) +
  geom_point(alpha = 0.7) +
  geom_smooth(method = "lm", se = TRUE, alpha = 0.2) +
  scale_y_log10(labels = comma_format(suffix = " ms/KB")) +
  scale_color_manual(values = compression_method_colors) +
  facet_wrap(~ Type, scales = "free") +
  labs(
    title = "Trade-off mezi kompresním ziskem a dobou zpracování",
    subtitle = "Poměr doby zpracování k velikosti souboru (časová složitost) vs. kompresní zisk (prostorová složitost)",
    x = "Kompresní zisk (%)",
    y = "Doba zpracování na KB (ms/KB, logaritmická stupnice)",
    color = "Metoda komprese"
  ) +
  my_theme

# B19: Analýza vlivu optimalizace na různé metriky
optimization_effect <- all_reports %>%
  pivot_longer(
    cols = c(CompressionRatio, CompressionTime, DecompressionTime, CompressionGainPercent),
    names_to = "Metric",
    values_to = "Value"
  ) %>%
  mutate(
    Metric = case_when(
      Metric == "CompressionRatio" ~ "Kompresní poměr",
      Metric == "CompressionTime" ~ "Doba komprese (ms)",
      Metric == "DecompressionTime" ~ "Doba dekomprese (ms)",
      Metric == "CompressionGainPercent" ~ "Kompresní zisk (%)"
    )
  )

optimization_plot <- ggplot(optimization_effect, 
                          aes(x = OptimizationCategory, y = Value, fill = MethodCategory)) +
  geom_boxplot(alpha = 0.7, position = "dodge") +
  facet_wrap(~ Metric, scales = "free_y") +
  scale_fill_manual(values = method_category_colors) +
  labs(
    title = "Vliv optimalizace na různé metriky podle kategorie linearizace",
    x = "Kategorie optimalizace",
    y = "Hodnota metriky",
    fill = "Kategorie linearizace"
  ) +
  my_theme



# ================================================================
# DALŠÍ DOPLŇUJÍCÍ GRAFY K ANALÝZE KOMPRESE STROMOVÝCH STRUKTUR
# ================================================================

# POZNÁMKA: Tyto grafy rozšiřují předchozí sady vizualizací - vložte je na konec
# vašeho skriptu pro úplnou analýzu

# ================================================================
# POKROČILÉ VIZUALIZACE
# ================================================================

# B1: Analýza stability kompresního poměru - směrodatná odchylka podle metody
stability_analysis <- all_reports %>%
  group_by(Dataset, Type) %>%
  summarise(
    MeanCompRatio = mean(CompressionRatio),
    StdDevCompRatio = sd(CompressionRatio),
    RangeCompRatio = max(CompressionRatio) - min(CompressionRatio),
    MedianCompRatio = median(CompressionRatio),
    IQR_CompRatio = IQR(CompressionRatio),
    .groups = "drop"
  )

stability_plot <- ggplot(stability_analysis, aes(x = Dataset, y = StdDevCompRatio, fill = Type)) +
  geom_bar(stat = "identity", position = "dodge", alpha = 0.8) +
  scale_fill_manual(values = file_type_colors) +
  labs(
    title = "Stabilita kompresního poměru podle metody a typu souboru",
    subtitle = "Nižší směrodatná odchylka znamená stabilnější výsledky komprese",
    x = "Metoda komprese",
    y = "Směrodatná odchylka kompresního poměru",
    fill = "Typ souboru"
  ) +
  my_theme +
  theme(axis.text.x = element_text(angle = 45, hjust = 1))

# B2: Jitter plot pro detailní zobrazení rozložení kompresních poměrů
jitter_plot <- ggplot(all_reports, aes(x = Type, y = CompressionRatio, color = Dataset)) +
  geom_jitter(position = position_jitterdodge(jitter.width = 0.2, dodge.width = 0.8), 
             alpha = 0.7, size = 2) +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  scale_color_manual(values = compression_method_colors) +
  labs(
    title = "Rozložení jednotlivých kompresních poměrů podle typu souboru a metody",
    subtitle = "Každý bod představuje jeden soubor",
    x = "Typ souboru",
    y = "Kompresní poměr",
    color = "Metoda komprese"
  ) +
  my_theme +
  theme(legend.position = "top")

# B3: Kombinovaný graf kompresního poměru (box + violin + jitter)
combined_ratio_plot <- ggplot(all_reports, aes(x = Type, y = CompressionRatio, fill = Dataset)) +
  geom_violin(alpha = 0.3, draw_quantiles = c(0.5), scale = "width", trim = TRUE) +
  geom_boxplot(width = 0.1, alpha = 0.8, outlier.shape = NA) +
  geom_jitter(position = position_jitterdodge(jitter.width = 0.1, dodge.width = 0.8), 
             alpha = 0.5, size = 1, aes(color = Dataset)) +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  scale_fill_manual(values = compression_method_colors) +
  scale_color_manual(values = compression_method_colors) +
  labs(
    title = "Komplexní vizualizace kompresního poměru",
    subtitle = "Kombinace violin plot, box plot a bodů pro jednotlivé soubory",
    x = "Typ souboru",
    y = "Kompresní poměr",
    fill = "Metoda komprese",
    color = "Metoda komprese"
  ) +
  my_theme +
  guides(color = "none")  # Skrýt duplicitní legendu pro barvu

# B4: Porovnání velikosti stromu napříč typy dokumentů
tree_size_comparison <- ggplot(all_reports, aes(x = Type, y = TreeSize, fill = Dataset)) +
  geom_boxplot(alpha = 0.7, position = "dodge") +
  scale_y_log10(labels = comma_format(suffix = " B")) +
  scale_fill_manual(values = compression_method_colors) +
  labs(
    title = "Porovnání velikosti stromu podle typu souboru a metody",
    x = "Typ souboru",
    y = "Velikost stromu (B, logaritmická stupnice)",
    fill = "Metoda komprese"
  ) +
  my_theme

# B5: Vizualizace kompresního poměru a doby zpracování jako dva grafy vedle sebe
side_by_side <- all_reports %>%
  pivot_longer(
    cols = c(CompressionRatio, TotalProcessingTime),
    names_to = "Metric",
    values_to = "Value"
  ) %>%
  mutate(
    Metric = case_when(
      Metric == "CompressionRatio" ~ "Kompresní poměr",
      Metric == "TotalProcessingTime" ~ "Celková doba zpracování (ms)"
    )
  )

side_by_side_plot <- ggplot(side_by_side, aes(x = Type, y = Value, fill = Dataset)) +
  geom_boxplot(alpha = 0.7, position = "dodge") +
  facet_wrap(~ Metric, scales = "free_y") +
  scale_fill_manual(values = compression_method_colors) +
  scale_y_continuous(
    sec.axis = sec_axis(~ ., name = "Celková doba zpracování (ms)"),
    labels = function(x) format(x, scientific = FALSE)
  ) +
  labs(
    title = "Porovnání kompresního poměru a celkové doby zpracování",
    x = "Typ souboru",
    y = "Hodnota",
    fill = "Metoda komprese"
  ) +
  my_theme

# B6: Rozložení kompresního poměru pro různé metody (více detailní histogramy)
detailed_histograms <- ggplot(all_reports, aes(x = CompressionRatio, fill = Dataset)) +
  geom_histogram(binwidth = 0.025, alpha = 0.7, position = "identity") +
  facet_grid(Dataset ~ Type, scales = "free_y") +
  scale_fill_manual(values = compression_method_colors) +
  geom_vline(xintercept = 1.0, linetype = "dashed", color = "darkred", size = 0.8) +
  labs(
    title = "Detailní histogramy kompresního poměru",
    subtitle = "Rozděleno podle metody komprese a typu souboru",
    x = "Kompresní poměr",
    y = "Počet souborů",
    fill = "Metoda komprese"
  ) +
  my_theme +
  theme(legend.position = "none")

# B7: Porovnání vlivu velikosti vs. typu souboru na kompresní poměr
size_vs_type_effect <- ggplot(all_reports, 
                             aes(x = Type, y = CompressionRatio, fill = SizeCategory)) +
  geom_boxplot(alpha = 0.7, position = "dodge") +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  scale_fill_viridis_d(option = "D", end = 0.8) +
  facet_wrap(~ Dataset) +
  labs(
    title = "Vliv typu souboru a velikostní kategorie na kompresní poměr",
    subtitle = "Rozděleno podle metody komprese",
    x = "Typ souboru",
    y = "Kompresní poměr",
    fill = "Velikostní kategorie"
  ) +
  my_theme +
  theme(axis.text.x = element_text(angle = 45, hjust = 1))

# B8: Analýza vlivu struktury stromu na kompresi - počet uzlů vs. kompresní poměr
tree_structure_influence <- ggplot(all_reports, 
                                 aes(x = TreeNodesCount, y = CompressionRatio, color = Dataset)) +
  geom_point(alpha = 0.7) +
  geom_smooth(method = "loess", se = TRUE, alpha = 0.2) +
  scale_x_log10(labels = comma_format()) +
  scale_color_manual(values = compression_method_colors) +
  facet_wrap(~ Type, scales = "free") +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  labs(
    title = "Vliv počtu uzlů stromu na kompresní poměr",
    x = "Počet uzlů stromu (logaritmická stupnice)",
    y = "Kompresní poměr",
    color = "Metoda komprese"
  ) +
  my_theme

# B9: Kompresní efektivita vzhledem k průměrné velikosti uzlu
all_reports <- all_reports %>%
  mutate(AvgNodeSize = ifelse(is.na(TreeNodesCount) | TreeNodesCount == 0, NA, TreeSize / TreeNodesCount))

node_size_effectiveness <- ggplot(
  all_reports %>% filter(!is.na(AvgNodeSize)), 
  aes(x = AvgNodeSize, y = CompressionRatio, color = Dataset)
) +
  geom_point(alpha = 0.7) +
  geom_smooth(method = "loess", se = TRUE, alpha = 0.2) +
  scale_x_log10(labels = comma_format(suffix = " B/uzel")) +
  scale_color_manual(values = compression_method_colors) +
  facet_wrap(~ Type, scales = "free") +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  labs(
    title = "Vliv průměrné velikosti uzlu na kompresní poměr",
    x = "Průměrná velikost uzlu (B/uzel, logaritmická stupnice)",
    y = "Kompresní poměr",
    color = "Metoda komprese"
  ) +
  my_theme

# B10: Trade-off mezi kompresním poměrem a rychlostí komprese
tradeoff_plot <- ggplot(all_reports, 
                      aes(x = CompressionRatio, y = CompressionTime, color = Dataset)) +
  geom_point(aes(size = Size), alpha = 0.7) +
  scale_y_log10(labels = comma_format(suffix = " ms")) +
  scale_color_manual(values = compression_method_colors) +
  scale_size_continuous(trans = "log10", range = c(2, 8), labels = comma_format()) +
  geom_vline(xintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  facet_wrap(~ Type, scales = "free_y") +
  labs(
    title = "Trade-off mezi kompresním poměrem a dobou komprese",
    subtitle = "Velikost bodů odpovídá velikosti souboru",
    x = "Kompresní poměr",
    y = "Doba komprese (ms, logaritmická stupnice)",
    color = "Metoda komprese",
    size = "Velikost souboru (B)"
  ) +
  my_theme

# B11: Porovnání kompresního poměru a poměru velikosti stromu k původní velikosti
ratio_comparison_plot <- ggplot(all_reports, 
                              aes(x = TreeToTextRatio, y = CompressionRatio, color = Dataset)) +
  geom_point(alpha = 0.7) +
  geom_smooth(method = "lm", se = TRUE, alpha = 0.2) +
  scale_x_log10(labels = comma_format()) +
  scale_color_manual(values = compression_method_colors) +
  facet_wrap(~ Type, scales = "free") +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  labs(
    title = "Vztah mezi poměrem velikosti stromu k textu a kompresním poměrem",
    x = "Poměr velikosti stromu / text (logaritmická stupnice)",
    y = "Kompresní poměr",
    color = "Metoda komprese"
  ) +
  my_theme

# B12: Porovnání efektivity pro malé, střední a velké soubory
size_category_box <- ggplot(all_reports, 
                          aes(x = SizeCategory, y = CompressionRatio, fill = Dataset)) +
  geom_boxplot(alpha = 0.7, position = "dodge") +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  scale_fill_manual(values = compression_method_colors) +
  facet_wrap(~ Type) +
  labs(
    title = "Kompresní poměr podle velikostní kategorie a typu souboru",
    subtitle = "Rozděleno podle metody komprese",
    x = "Velikostní kategorie",
    y = "Kompresní poměr",
    fill = "Metoda komprese"
  ) +
  my_theme +
  theme(axis.text.x = element_text(angle = 45, hjust = 1))

# B13: Spider (radar) chart pro komplexní porovnání metod
# Připravit data pro spider chart - průměrné hodnoty po normalizaci
radar_data_prep <- all_reports %>%
  group_by(Dataset) %>%
  summarise(
    AvgCompRatio = mean(CompressionRatio),
    AvgCompTime = mean(CompressionTime),
    AvgDecompTime = mean(DecompressionTime),
    AvgGainPercent = mean(CompressionGainPercent),
    AvgPerformance = mean(CompressionPerformance),
    .groups = "drop"
  )

# Normalizace hodnot pro spider chart (0-1 škála)
radar_data_normalized <- radar_data_prep %>%
  mutate(
    CompRatio_Norm = 1 - ((AvgCompRatio - min(AvgCompRatio)) / (max(AvgCompRatio) - min(AvgCompRatio))),
    CompTime_Norm = 1 - ((AvgCompTime - min(AvgCompTime)) / (max(AvgCompTime) - min(AvgCompTime))),
    DecompTime_Norm = 1 - ((AvgDecompTime - min(AvgDecompTime)) / (max(AvgDecompTime) - min(AvgDecompTime))),
    GainPercent_Norm = (AvgGainPercent - min(AvgGainPercent)) / (max(AvgGainPercent) - min(AvgGainPercent)),
    Performance_Norm = (AvgPerformance - min(AvgPerformance)) / (max(AvgPerformance) - min(AvgPerformance))
  ) %>%
  select(Dataset, ends_with("_Norm")) %>%
  pivot_longer(
    cols = -Dataset,
    names_to = "Metric",
    values_to = "Value"
  ) %>%
  mutate(
    Metric = case_when(
      Metric == "CompRatio_Norm" ~ "Kompresní poměr",
      Metric == "CompTime_Norm" ~ "Doba komprese",
      Metric == "DecompTime_Norm" ~ "Doba dekomprese",
      Metric == "GainPercent_Norm" ~ "Kompresní zisk",
      Metric == "Performance_Norm" ~ "Výkon komprese"
    )
  )

# Spider chart pro porovnání metod
spider_chart <- ggplot(radar_data_normalized, 
                      aes(x = Metric, y = Value, color = Dataset, group = Dataset)) +
  geom_polygon(aes(fill = Dataset), alpha = 0.1) +
  geom_line(size = 1) +
  geom_point(size = 3) +
  scale_color_manual(values = compression_method_colors) +
  scale_fill_manual(values = compression_method_colors) +
  coord_polar() +
  labs(
    title = "Porovnání kompresních metod - Spider Chart",
    subtitle = "Vyšší hodnoty jsou lepší pro všechny metriky (hodnoty normalizovány)",
    color = "Metoda komprese",
    fill = "Metoda komprese"
  ) +
  my_theme +
  theme(
    axis.text.y = element_blank(),
    axis.ticks.y = element_blank(),
    panel.grid.major.y = element_line(color = "gray90"),
    panel.grid.minor.y = element_blank()
  )

# B14: Analýza optimality - křivka Pareto fronty pro čas komprese vs. kompresní poměr
# Vytvoření jednoduchého scatter plotu
pareto_data <- all_reports %>%
  select(FileName, Dataset, Type, Size, CompressionRatio, CompressionTime)

# Funkce pro identifikaci Pareto optimálních bodů
identify_pareto <- function(df) {
  df_sorted <- df %>%
    arrange(CompressionRatio, CompressionTime)
  
  pareto_points <- c(1)
  current_min_time <- df_sorted$CompressionTime[1]
  
  for (i in 2:nrow(df_sorted)) {
    if (df_sorted$CompressionTime[i] < current_min_time) {
      pareto_points <- c(pareto_points, i)
      current_min_time <- df_sorted$CompressionTime[i]
    }
  }
  
  df_sorted$IsParetoOptimal <- FALSE
  df_sorted$IsParetoOptimal[pareto_points] <- TRUE
  
  return(df_sorted)
}

# Aplikace Pareto analýzy pro každý typ souboru
pareto_results <- list()
for (type in unique(pareto_data$Type)) {
  type_data <- pareto_data %>% filter(Type == type)
  pareto_results[[type]] <- identify_pareto(type_data)
}

pareto_all <- bind_rows(pareto_results)

# Vytvoření Pareto grafu
pareto_plot <- ggplot(pareto_all, 
                     aes(x = CompressionRatio, y = CompressionTime, color = Dataset)) +
  geom_point(aes(size = Size, shape = IsParetoOptimal), alpha = 0.7) +
  scale_y_log10(labels = comma_format(suffix = " ms")) +
  scale_color_manual(values = compression_method_colors) +
  scale_shape_manual(values = c(16, 17)) +
  scale_size_continuous(trans = "log10", range = c(2, 8), labels = comma_format()) +
  facet_wrap(~ Type, scales = "free") +
  labs(
    title = "Pareto optimální řešení: Kompresní poměr vs. Doba komprese",
    subtitle = "Trojúhelníky označují Pareto optimální body",
    x = "Kompresní poměr",
    y = "Doba komprese (ms, logaritmická stupnice)",
    color = "Metoda komprese",
    size = "Velikost souboru (B)",
    shape = "Pareto optimální"
  ) +
  my_theme

# B15: Heatmapa průměrného kompresního poměru podle typu souboru a metody
heatmap_comp_ratio <- all_reports %>%
  group_by(Dataset, Type) %>%
  summarise(
    AvgCompRatio = mean(CompressionRatio),
    .groups = "drop"
  )

heatmap_plot <- ggplot(heatmap_comp_ratio, aes(x = Type, y = Dataset, fill = AvgCompRatio)) +
  geom_tile(color = "white") +
  scale_fill_gradient2(low = "blue", mid = "white", high = "red", 
                     midpoint = 1, limits = c(min(heatmap_comp_ratio$AvgCompRatio), 
                                            max(heatmap_comp_ratio$AvgCompRatio))) +
  geom_text(aes(label = sprintf("%.2f", AvgCompRatio)), color = "black", size = 4) +
  labs(
    title = "Heatmapa průměrných kompresních poměrů",
    subtitle = "Podle typu souboru a metody komprese",
    x = "Typ souboru",
    y = "Metoda komprese",
    fill = "Průměrný\nkompresní poměr"
  ) +
  my_theme +
  theme(
    axis.text.x = element_text(angle = 45, hjust = 1),
    panel.grid.major = element_blank(),
    panel.grid.minor = element_blank()
  )

# B16: Porovnání distribuční funkce CDF kompresního poměru
cdf_plot <- ggplot(all_reports, aes(x = CompressionRatio, color = Dataset)) +
  stat_ecdf(geom = "step", pad = FALSE, size = 1) +
  geom_vline(xintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  scale_color_manual(values = compression_method_colors) +
  facet_wrap(~ Type, scales = "free") +
  labs(
    title = "Kumulativní distribuční funkce (CDF) kompresního poměru",
    subtitle = "Vyšší křivka u hodnoty 1.0 značí lepší metodu (více souborů pod hranicí úspěšné komprese)",
    x = "Kompresní poměr",
    y = "Kumulativní relativní četnost",
    color = "Metoda komprese"
  ) +
  my_theme

# B17: Porovnání relativního zlepšení optimalizovaných metod
# Vypočítat průměrné hodnoty pro základní a optimalizované metody
improvement_data <- all_reports %>%
  mutate(
    OptimizationPair = case_when(
      Dataset == "Linearizace + RePair" ~ "LinearizacePair",
      Dataset == "Opt. linearizace + RePair" ~ "LinearizacePair",
      Dataset == "Komprese bez linearizace" ~ "BezLinearizacePair",
      Dataset == "Opt. komprese bez linearizace" ~ "BezLinearizacePair"
    ),
    IsOptimized = case_when(
      Dataset %in% c("Opt. linearizace + RePair", "Opt. komprese bez linearizace") ~ "Optimalizovaná",
      TRUE ~ "Základní"
    )
  ) %>%
  group_by(Type, OptimizationPair, IsOptimized) %>%
  summarise(
    AvgCompRatio = mean(CompressionRatio),
    AvgCompTime = mean(CompressionTime),
    AvgGainPercent = mean(CompressionGainPercent),
    .groups = "drop"
  ) %>%
  pivot_wider(
    names_from = IsOptimized,
    values_from = c(AvgCompRatio, AvgCompTime, AvgGainPercent)
  ) %>%
  mutate(
    CompRatio_Improvement = (AvgCompRatio_Základní - AvgCompRatio_Optimalizovaná) / AvgCompRatio_Základní * 100,
    CompTime_Improvement = (AvgCompTime_Základní - AvgCompTime_Optimalizovaná) / AvgCompTime_Základní * 100,
    GainPercent_Improvement = (AvgGainPercent_Optimalizovaná - AvgGainPercent_Základní) / AvgGainPercent_Základní * 100
  )

# Převod do dlouhého formátu pro grafické znázornění
improvement_long <- improvement_data %>%
  select(Type, OptimizationPair, ends_with("_Improvement")) %>%
  pivot_longer(
    cols = ends_with("_Improvement"),
    names_to = "Metric",
    values_to = "ImprovementPercent"
  ) %>%
  mutate(
    OptimizationPair = case_when(
      OptimizationPair == "LinearizacePair" ~ "Linearizace + RePair",
      OptimizationPair == "BezLinearizacePair" ~ "Komprese bez linearizace"
    ),
    Metric = case_when(
      Metric == "CompRatio_Improvement" ~ "Zlepšení kompresního poměru",
      Metric == "CompTime_Improvement" ~ "Zlepšení doby komprese",
      Metric == "GainPercent_Improvement" ~ "Zlepšení kompresního zisku"
    )
  )

# Graf zlepšení
improvement_plot <- ggplot(improvement_long, 
                         aes(x = Type, y = ImprovementPercent, fill = OptimizationPair)) +
  geom_bar(stat = "identity", position = "dodge", alpha = 0.8) +
  geom_text(aes(label = sprintf("%.1f%%", ImprovementPercent)), 
           position = position_dodge(width = 0.9), vjust = -0.3, size = 3) +
  facet_wrap(~ Metric, scales = "free_y") +
  scale_fill_manual(values = c("Linearizace + RePair" = "#1F78B4", 
                              "Komprese bez linearizace" = "#33A02C")) +
  labs(
    title = "Relativní zlepšení optimalizovaných metod oproti základním",
    subtitle = "Kladné hodnoty znamenají zlepšení, záporné hodnoty zhoršení",
    x = "Typ souboru",
    y = "Relativní zlepšení (%)",
    fill = "Skupina metod"
  ) +
  my_theme +
  theme(axis.text.x = element_text(angle = 45, hjust = 1))

# B18: Porovnání "trade-off" mezi prostorovou a časovou složitostí
tradeoff_space_time <- ggplot(all_reports, 
                            aes(x = CompressionGainPercent, y = TotalProcessingTime / Size * 1000, 
                               color = Dataset)) +
  geom_point(alpha = 0.7) +
  geom_smooth(method = "lm", se = TRUE, alpha = 0.2) +
  scale_y_log10(labels = comma_format(suffix = " ms/KB")) +
  scale_color_manual(values = compression_method_colors) +
  facet_wrap(~ Type, scales = "free") +
  labs(
    title = "Trade-off mezi kompresním ziskem a dobou zpracování",
    subtitle = "Poměr doby zpracování k velikosti souboru (časová složitost) vs. kompresní zisk (prostorová složitost)",
    x = "Kompresní zisk (%)",
    y = "Doba zpracování na KB (ms/KB, logaritmická stupnice)",
    color = "Metoda komprese"
  ) +
  my_theme

# B19: Analýza vlivu optimalizace na různé metriky
optimization_effect <- all_reports %>%
  pivot_longer(
    cols = c(CompressionRatio, CompressionTime, DecompressionTime, CompressionGainPercent),
    names_to = "Metric",
    values_to = "Value"
  ) %>%
  mutate(
    Metric = case_when(
      Metric == "CompressionRatio" ~ "Kompresní poměr",
      Metric == "CompressionTime" ~ "Doba komprese (ms)",
      Metric == "DecompressionTime" ~ "Doba dekomprese (ms)",
      Metric == "CompressionGainPercent" ~ "Kompresní zisk (%)"
    )
  )

optimization_plot <- ggplot(optimization_effect, 
                          aes(x = OptimizationCategory, y = Value, fill = MethodCategory)) +
  geom_boxplot(alpha = 0.7, position = "dodge") +
  facet_wrap(~ Metric, scales = "free_y") +
  scale_fill_manual(values = method_category_colors) +
  labs(
    title = "Vliv optimalizace na různé metriky podle kategorie linearizace",
    x = "Kategorie optimalizace",
    y = "Hodnota metriky",
    fill = "Kategorie linearizace"
  ) +
  my_theme

  ) %>%
  mutate(
    Metric = case_when(
      Metric == "AvgCompRatio" ~ "Průměrný kompresní poměr",
      Metric == "MedianCompRatio" ~ "Mediánový kompresní poměr",
      Metric == "AvgCompTime" ~ "Průměrná doba komprese (ms)",
      Metric == "AvgGainPercent" ~ "Průměrný kompresní zisk (%)",
      Metric == "SuccessRate" ~ "Úspěšnost komprese (%)"
    )
  )

# Normalizace hodnot pro lepší vizualizaci
overview_metrics_normalized <- overview_metrics %>%
  group_by(Metric) %>%
  mutate(
    NormalizedValue = case_when(
      Metric %in% c("Průměrný kompresní poměr", "Mediánový kompresní poměr") ~ 
        1 - (Value - min(Value)) / (max(Value) - min(Value)),
      Metric == "Průměrná doba komprese (ms)" ~ 
        1 - (Value - min(Value)) / (max(Value) - min(Value)),
      TRUE ~ (Value - min(Value)) / (max(Value) - min(Value))
    )
  ) %>%
  ungroup()

# Vytvoření přehledového tile plotu
overview_tile_plot <- ggplot(overview_metrics_normalized, 
                           aes(x = Metric, y = Dataset, fill = NormalizedValue)) +
  geom_tile(color = "white") +
  scale_fill_gradient(low = "white", high = "steelblue") +
  geom_text(aes(label = case_when(
    Metric %in% c("Průměrný kompresní poměr", "Mediánový kompresní poměr") ~ sprintf("%.2f", Value),
    Metric == "Průměrná doba komprese (ms)" ~ sprintf("%.0f", Value),
    TRUE ~ sprintf("%.1f%%", Value)
  )), color = "black", size = 3.5) +
  labs(
    title = "Přehled klíčových metrik pro jednotlivé metody komprese",
    subtitle = "Tmavší barva značí lepší výkon v dané metrice po normalizaci",
    x = "",
    y = "",
    fill = "Normalizovaná\nhodnota"
  ) +
  my_theme +
  theme(
    axis.text.x = element_text(angle = 45, hjust = 1),
    panel.grid.major = element_blank(),
    panel.grid.minor = element_blank()
  )

# ================================================================
# GRAFY VYHODNOCUJÍCÍ OPTIMÁLNÍ VOLBU METODY ####
# ================================================================

# C1: Porovnání rychlosti podle velikosti souboru - rozhodovací graf
speed_by_size <- ggplot(all_reports, 
                      aes(x = Size, y = CompressionTime / Size * 1000, color = Dataset)) +
  geom_point(alpha = 0.6) +
  geom_smooth(method = "loess", se = TRUE, alpha = 0.2) +
  scale_x_log10(labels = comma_format(suffix = " B")) +
  scale_y_log10(labels = comma_format(suffix = " ms/KB")) +
  scale_color_manual(values = compression_method_colors) +
  labs(
    title = "Doba komprese na KB podle velikosti souboru",
    subtitle = "Pomáhá určit, která metoda je nejefektivnější pro různé velikosti souborů",
    x = "Velikost souboru (B, logaritmická stupnice)",
    y = "Doba komprese na KB (ms/KB, logaritmická stupnice)",
    color = "Metoda komprese"
  ) +
  my_theme +
  facet_wrap(~ Type, scales = "free")

# C2: Průměrné kompresní poměry napříč velikostními kategoriemi a typy souborů
avg_ratio_by_size_type <- all_reports %>%
  group_by(Dataset, SizeCategory, Type) %>%
  summarise(
    AvgCompRatio = mean(CompressionRatio),
    Count = n(),
    .groups = "drop"
  ) %>%
  filter(Count >= 3)  # Pouze skupiny s dostatečným počtem vzorků

matrix_plot <- ggplot(avg_ratio_by_size_type, 
                    aes(x = SizeCategory, y = Type, fill = AvgCompRatio)) +
  geom_tile(color = "white") +
  facet_wrap(~ Dataset) +
  scale_fill_gradient2(low = "blue", mid = "white", high = "red", 
                     midpoint = 1, limits = c(min(avg_ratio_by_size_type$AvgCompRatio), 
                                            max(avg_ratio_by_size_type$AvgCompRatio))) +
  geom_text(aes(label = sprintf("%.2f", AvgCompRatio)), color = "black", size = 3) +
  labs(
    title = "Průměrné kompresní poměry podle velikostní kategorie a typu souboru",
    subtitle = "Rozděleno podle metody komprese",
    x = "Velikostní kategorie",
    y = "Typ souboru",
    fill = "Průměrný\nkompresní poměr"
  ) +
  my_theme +
  theme(
    axis.text.x = element_text(angle = 45, hjust = 1),
    panel.grid.major = element_blank(),
    panel.grid.minor = element_blank()
  )

# C3: Složený sloupcový graf pro porovnání úspěšnosti komprese podle typu a metody
success_rate_data <- all_reports %>%
  mutate(CompressionSuccess = ifelse(CompressionRatio < 1, "Úspěšná", "Neúspěšná")) %>%
  group_by(Dataset, Type, CompressionSuccess) %>%
  summarise(Count = n(), .groups = "drop") %>%
  group_by(Dataset, Type) %>%
  mutate(Total = sum(Count), Percentage = Count / Total * 100) %>%
  ungroup()

success_rate_plot <- ggplot(success_rate_data, 
                          aes(x = Type, y = Percentage, fill = CompressionSuccess)) +
  geom_bar(stat = "identity", position = "stack") +
  geom_text(aes(label = sprintf("%.0f%%", Percentage)), 
           position = position_stack(vjust = 0.5), color = "white", fontface = "bold") +
  facet_wrap(~ Dataset) +
  scale_fill_manual(values = c("Úspěšná" = "#4DAF4A", "Neúspěšná" = "#E41A1C")) +
  labs(
    title = "Podíl úspěšně a neúspěšně komprimovaných souborů",
    subtitle = "Podle typu souboru a metody komprese",
    x = "Typ souboru",
    y = "Podíl souborů (%)",
    fill = "Výsledek komprese"
  ) +
  my_theme +
  theme(axis.text.x = element_text(angle = 45, hjust = 1))

# C4: Analýza kompresní efektivity vzhledem k vlastnostem stromu
# Vytvoření indexu komplexity stromu (zjednodušený model)
all_reports <- all_reports %>%
  mutate(
    TreeComplexityIndex = ifelse(is.na(TreeNodesCount) | TreeNodesCount == 0, NA,
                               TreeNodesCount * log10(TreeSize / TreeNodesCount + 1))
  )

tree_complexity_plot <- ggplot(
  all_reports %>% filter(!is.na(TreeComplexityIndex)), 
  aes(x = TreeComplexityIndex, y = CompressionRatio, color = Dataset)
) +
  geom_point(alpha = 0.7) +
  geom_smooth(method = "loess", se = TRUE, alpha = 0.2) +
  scale_x_log10(labels = comma_format()) +
  scale_color_manual(values = compression_method_colors) +
  facet_wrap(~ Type, scales = "free") +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  labs(
    title = "Vliv indexu komplexity stromu na kompresní poměr",
    subtitle = "Index komplexity = počet uzlů * log10(průměrná velikost uzlu + 1)",
    x = "Index komplexity stromu (logaritmická stupnice)",
    y = "Kompresní poměr",
    color = "Metoda komprese"
  ) +
  my_theme

# C5: Kombninace kompresního poměru a doby komprese v jednotném grafu
combined_ratio_time <- all_reports %>%
  group_by(Dataset, Type) %>%
  summarise(
    AvgCompRatio = mean(CompressionRatio),
    AvgCompTime = mean(CompressionTime),
    .groups = "drop"
  )

combined_plot <- ggplot(combined_ratio_time, 
                      aes(x = AvgCompRatio, y = AvgCompTime, color = Dataset, shape = Type)) +
  geom_point(size = 4) +
  geom_text_repel(aes(label = Type), 
                box.padding = 0.5, point.padding = 0.5, 
                segment.color = "grey50", size = 3) +
  scale_y_log10(labels = comma_format(suffix = " ms")) +
  scale_color_manual(values = compression_method_colors) +
  geom_vline(xintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  labs(
    title = "Porovnání metod podle kompresního poměru a doby komprese",
    subtitle = "Ideální metoda je v levém dolním rohu (nízký kompresní poměr, krátká doba komprese)",
    x = "Průměrný kompresní poměr",
    y = "Průměrná doba komprese (ms, logaritmická stupnice)",
    color = "Metoda komprese",
    shape = "Typ souboru"
  ) +
  my_theme +
  theme(legend.position = "right")

# C6: Metoda komprese jako faktor - porovnání vlivu
methods_as_factor <- all_reports %>%
  group_by(Dataset, Type) %>%
  summarise(
    AvgCompRatio = mean(CompressionRatio),
    AvgCompTime = mean(CompressionTime),
    AvgGainPercent = mean(CompressionGainPercent),
    .groups = "drop"
  ) %>%
  pivot_longer(
    cols = c(AvgCompRatio, AvgCompTime, AvgGainPercent),
    names_to = "Metric",
    values_to = "Value"
  ) %>%
  mutate(
    Metric = case_when(
      Metric == "AvgCompRatio" ~ "Průměrný kompresní poměr",
      Metric == "AvgCompTime" ~ "Průměrná doba komprese (ms)",
      Metric == "AvgGainPercent" ~ "Průměrný kompresní zisk (%)"
    )
  )

method_as_factor_plot <- ggplot(methods_as_factor, 
                              aes(x = Type, y = Value, fill = Dataset, group = Dataset)) +
  geom_bar(stat = "identity", position = "dodge") +
  facet_wrap(~ Metric, scales = "free_y") +
  scale_fill_manual(values = compression_method_colors) +
  labs(
    title = "Vliv metody komprese jako faktoru na různé metriky",
    x = "Typ souboru",
    y = "Hodnota metriky",
    fill = "Metoda komprese"
  ) +
  my_theme +
  theme(axis.text.x = element_text(angle = 45, hjust = 1))

# C7: Agregovaný přehled pro manažerskou prezentaci
management_summary <- all_reports %>%
  group_by(Dataset) %>%
  summarise(
    AvgCompRatio = mean(CompressionRatio),
    MedianCompRatio = median(CompressionRatio),
    AvgCompTime = mean(CompressionTime),
    AvgGainPercent = mean(CompressionGainPercent),
    SuccessRate = sum(CompressionRatio < 1) / n() * 100,
    TimePerMB = mean(CompressionTime / (Size/1024/1024)),
    .groups = "drop"
  )

# C8: Lollipop graf pro srovnání kompresních faktorů
lollipop_data <- all_reports %>%
  group_by(Dataset, Type) %>%
  summarise(
    AvgCompFactor = mean(CompressionFactor),
    .groups = "drop"
  )

lollipop_plot <- ggplot(lollipop_data, aes(x = AvgCompFactor, y = Type, color = Dataset)) +
  geom_segment(aes(x = 0, xend = AvgCompFactor, yend = Type), 
              size = 1.5, alpha = 0.7) +
  geom_point(size = 4) +
  scale_x_continuous(limits = c(0, max(lollipop_data$AvgCompFactor) * 1.1)) +
  scale_color_manual(values = compression_method_colors) +
  facet_wrap(~ Dataset) +
  labs(
    title = "Porovnání průměrných kompresních faktorů podle typu souboru a metody",
    subtitle = "Vyšší hodnoty znamenají lepší kompresi",
    x = "Průměrný kompresní faktor",
    y = "Typ souboru",
    color = "Metoda komprese"
  ) +
  my_theme +
  theme(legend.position = "none")

# C9: Časové náročnost různých fází komprese
compression_phases <- all_reports %>%
  group_by(Dataset, Type) %>%
  summarise(
    AvgTextToTree = mean(TextToTreeDuration),
    AvgCompression = mean(CompressionTime - TextToTreeDuration),
    AvgDecompression = mean(DecompressionTime),
    .groups = "drop"
  ) %>%
  pivot_longer(
    cols = c(AvgTextToTree, AvgCompression, AvgDecompression),
    names_to = "Phase",
    values_to = "Duration"
  ) %>%
  mutate(
    Phase = case_when(
      Phase == "AvgTextToTree" ~ "Převod textu na strom",
      Phase == "AvgCompression" ~ "Vlastní komprese",
      Phase == "AvgDecompression" ~ "Dekomprese"
    )
  )

phases_plot <- ggplot(compression_phases, 
                    aes(x = Type, y = Duration, fill = Phase)) +
  geom_bar(stat = "identity", position = "stack") +
  facet_wrap(~ Dataset) +
  scale_fill_brewer(palette = "Set2") +
  labs(
    title = "Časová náročnost jednotlivých fází zpracování",
    subtitle = "Podle typu souboru a metody komprese",
    x = "Typ souboru",
    y = "Doba trvání (ms)",
    fill = "Fáze zpracování"
  ) +
  my_theme +
  theme(axis.text.x = element_text(angle = 45, hjust = 1))

# C10: Optimální metoda pro různé scénáře - rozhodovací matice
# Vytvoření skóre pro různé scénáře
scenario_scoring <- all_reports %>%
  group_by(Dataset) %>%
  summarise(
    CompressRatioScore = 1 - (mean(CompressionRatio) - min(all_reports$CompressionRatio)) / 
      (max(all_reports$CompressionRatio) - min(all_reports$CompressionRatio)),
    SpeedScore = 1 - (mean(CompressionTime) - min(all_reports$CompressionTime)) / 
      (max(all_reports$CompressionTime) - min(all_reports$CompressionTime)),
    DecompSpeedScore = 1 - (mean(DecompressionTime) - min(all_reports$DecompressionTime)) / 
      (max(all_reports$DecompressionTime) - min(all_reports$DecompressionTime)),
    .groups = "drop"
  ) %>%
  mutate(
    BalancedScore = (CompressRatioScore + SpeedScore + DecompSpeedScore) / 3,
    SpaceOptimizedScore = (CompressRatioScore * 0.7 + SpeedScore * 0.15 + DecompSpeedScore * 0.15),
    SpeedOptimizedScore = (CompressRatioScore * 0.15 + SpeedScore * 0.7 + DecompSpeedScore * 0.15),
    DecompOptimizedScore = (CompressRatioScore * 0.15 + SpeedScore * 0.15 + DecompSpeedScore * 0.7)
  ) %>%
  select(Dataset, BalancedScore, SpaceOptimizedScore, SpeedOptimizedScore, DecompOptimizedScore) %>%
  pivot_longer(
    cols = ends_with("Score"),
    names_to = "Scenario",
    values_to = "Score"
  ) %>%
  mutate(
    Scenario = case_when(
      Scenario == "BalancedScore" ~ "Vyvážený scénář",
      Scenario == "SpaceOptimizedScore" ~ "Úspora místa",
      Scenario == "SpeedOptimizedScore" ~ "Rychlost komprese",
      Scenario == "DecompOptimizedScore" ~ "Rychlost dekomprese"
    )
  )

# Nalezení nejlepších metod pro každý scénář
best_methods <- scenario_scoring %>%
  group_by(Scenario) %>%
  filter(Score == max(Score)) %>%
  ungroup()

# Vytvoření matice skóre
scenario_heatmap <- ggplot(scenario_scoring, 
                         aes(x = Scenario, y = Dataset, fill = Score)) +
  geom_tile(color = "white") +
  scale_fill_gradient(low = "white", high = "darkgreen") +
  geom_text(aes(label = sprintf("%.2f", Score)), color = "black", size = 4) +
  geom_tile(data = best_methods, 
           aes(x = Scenario, y = Dataset), 
           fill = NA, color = "gold", size = 1.5) +
  labs(
    title = "Optimální metody pro různé scénáře použití",
    subtitle = "Zlatý rámeček označuje nejlepší metodu pro daný scénář",
    x = "Scénář použití",
    y = "Metoda komprese",
    fill = "Skóre"
  ) +
  my_theme +
  theme(
    axis.text.x = element_text(angle = 45, hjust = 1),
    panel.grid.major = element_blank(),
    panel.grid.minor = element_blank()
  )

# ================================================================
# SLOŽENÉ GRAFY A MULTIFAKTOROVÉ ANALÝZY ####
# ================================================================

# D1: Analýza vzájemných interakcí mezi různými faktory
# Příprava dat pro analýzu interakcí
interaction_data <- all_reports %>%
  group_by(MethodCategory, OptimizationCategory, Type, SizeCategory) %>%
  summarise(
    AvgCompRatio = mean(CompressionRatio),
    Count = n(),
    .groups = "drop"
  ) %>%
  filter(Count >= 3)  # Pouze skupiny s dostatečným počtem vzorků

# Interakce mezi kategorií metody a optimalizace
method_opt_interaction <- ggplot(interaction_data, 
                              aes(x = MethodCategory, y = AvgCompRatio, color = OptimizationCategory, 
                                 group = OptimizationCategory)) +
  geom_point(size = 3) +
  geom_line(size = 1) +
  facet_grid(SizeCategory ~ Type) +
  scale_color_manual(values = optimization_category_colors) +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 0.8) +
  labs(
    title = "Interakce mezi kategorií metody a optimalizace",
    subtitle = "Rozděleno podle typu souboru a velikostní kategorie",
    x = "Kategorie metody",
    y = "Průměrný kompresní poměr",
    color = "Kategorie optimalizace"
  ) +
  my_theme +
  theme(axis.text.x = element_text(angle = 45, hjust = 1))

# D2: Celkové pořadí metod podle vážených kritérií
# Vytvoření vážených skóre pro různé metriky
weighted_ranking <- all_reports %>%
  group_by(Dataset) %>%
  summarise(
    CompRatioScore = 1 - (mean(CompressionRatio) - min(all_reports$CompressionRatio)) / 
      (max(all_reports$CompressionRatio) - min(all_reports$CompressionRatio)),
    TimeScore = 1 - (mean(CompressionTime) - min(all_reports$CompressionTime)) / 
      (max(all_reports$CompressionTime) - min(all_reports$CompressionTime)),
    GainScore = (mean(CompressionGainPercent) - min(all_reports$CompressionGainPercent)) / 
      (max(all_reports$CompressionGainPercent) - min(all_reports$CompressionGainPercent)),
    StabilityScore = 1 - (sd(CompressionRatio) - min(c(sd(all_reports$CompressionRatio)))) / 
      (max(c(sd(all_reports$CompressionRatio))) - min(c(sd(all_reports$CompressionRatio)))),
    SuccessScore = sum(CompressionRatio < 1) / n(),
    .groups = "drop"
  ) %>%
  mutate(
    WeightedScore = CompRatioScore * 0.35 + TimeScore * 0.25 + 
      GainScore * 0.2 + StabilityScore * 0.1 + SuccessScore * 0.1
  ) %>%
  arrange(desc(WeightedScore))

# Vytvoření celkového hodnocení
ranking_plot <- ggplot(weighted_ranking, aes(x = reorder(Dataset, WeightedScore), y = WeightedScore)) +
  geom_bar(stat = "identity", fill = "steelblue", width = 0.7) +
  geom_text(aes(label = sprintf("%.2f", WeightedScore)), hjust = -0.2) +
  coord_flip() +
  scale_y_continuous(limits = c(0, max(weighted_ranking$WeightedScore) * 1.2)) +
  labs(
    title = "Celkové hodnocení metod komprese",
    subtitle = "Vážené skóre: kompresní poměr (35%), rychlost (25%), zisk (20%), stabilita (10%), úspěšnost (10%)",
    x = "Metoda komprese",
    y = "Vážené skóre"
  ) +
  my_theme

# D3: Detailní breakdown skóre podle kategorií
ranking_breakdown <- weighted_ranking %>%
  select(Dataset, CompRatioScore, TimeScore, GainScore, StabilityScore, SuccessScore) %>%
  pivot_longer(
    cols = -Dataset,
    names_to = "Category",
    values_to = "Score"
  ) %>%
  mutate(
    Category = case_when(
      Category == "CompRatioScore" ~ "Kompresní poměr",
      Category == "TimeScore" ~ "Rychlost komprese",
      Category == "GainScore" ~ "Kompresní zisk",
      Category == "StabilityScore" ~ "Stabilita komprese",
      Category == "SuccessScore" ~ "Úspěšnost komprese"
    )
  )

breakdown_plot <- ggplot(ranking_breakdown, 
                       aes(x = reorder(Dataset, Score, FUN = function(x) sum(x)), 
                          y = Score, fill = Category)) +
  geom_bar(stat = "identity", position = "stack") +
  coord_flip() +
  scale_fill_brewer(palette = "Set3") +
  labs(
    title = "Breakdown celkového hodnocení metod komprese",
    subtitle = "Rozděleno podle kategorií hodnocení",
    x = "Metoda komprese",
    y = "Skóre",
    fill = "Kategorie hodnocení"
  ) +
  my_theme

# D4: Porovnání průměrné vs. mediánové hodnoty (kompresní poměr)
avg_median_comparison <- all_reports %>%
  group_by(Dataset, Type) %>%
  summarise(
    AvgCompRatio = mean(CompressionRatio),
    MedianCompRatio = median(CompressionRatio),
    .groups = "drop"
  ) %>%
  pivot_longer(
    cols = c(AvgCompRatio, MedianCompRatio),
    names_to = "Statistic",
    values_to = "Value"
  ) %>%
  mutate(
    Statistic = case_when(
      Statistic == "AvgCompRatio" ~ "Průměrný kompresní poměr",
      Statistic == "MedianCompRatio" ~ "Mediánový kompresní poměr"
    )
  )

avg_median_plot <- ggplot(avg_median_comparison, 
                        aes(x = Type, y = Value, fill = Statistic)) +
  geom_bar(stat = "identity", position = "dodge") +
  facet_wrap(~ Dataset) +
  scale_fill_manual(values = c("Průměrný kompresní poměr" = "#66A61E", 
                              "Mediánový kompresní poměr" = "#E6AB02")) +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 0.8) +
  labs(
    title = "Porovnání průměrného a mediánového kompresního poměru",
    subtitle = "Rozdíl indikuje přítomnost odlehlých hodnot",
    x = "Typ souboru",
    y = "Kompresní poměr",
    fill = "Statistika"
  ) +
  my_theme +
  theme(axis.text.x = element_text(angle = 45, hjust = 1))

# D5: Distribuce kompresních poměrů pomocí hustotních křivek s vyznačenými kvantily
density_with_quantiles <- ggplot(all_reports, aes(x = CompressionRatio, color = Dataset, fill = Dataset)) +
  geom_density(alpha = 0.2) +
  geom_vline(aes(xintercept = 1.0), linetype = "dashed", color = "darkred", size = 1) +
  geom_vline(data = all_reports %>% 
               group_by(Dataset) %>% 
               summarise(q25 = quantile(CompressionRatio, 0.25),
                         median = median(CompressionRatio),
                         q75 = quantile(CompressionRatio, 0.75),
                         .groups = "drop") %>%
               pivot_longer(cols = c(q25, median, q75), 
                           names_to = "Quantile", 
                           values_to = "Value"),
             aes(xintercept = Value, color = Dataset), 
             linetype = "dotted", size = 0.8) +
  scale_color_manual(values = compression_method_colors) +
  scale_fill_manual(values = compression_method_colors) +
  facet_wrap(~ Type) +
  labs(
    title = "Distribuce kompresních poměrů s vyznačenými kvantily",
    subtitle = "Tečkované čáry značí 25%, 50% a 75% kvantily pro každou metodu",
    x = "Kompresní poměr",
    y = "Hustota",
    color = "Metoda komprese",
    fill = "Metoda komprese"
  ) +
  my_theme



# ================================================================
# UKLÁDÁNÍ VŠECH ZBÝVAJÍCÍCH GRAFŮ
# ================================================================

# Uložení grafů ze sady B
save_plot(stability_plot, "b11_stability_plot", width = 14, height = 8)
save_plot(jitter_plot, "b12_jitter_plot", width = 14, height = 8)
save_plot(combined_ratio_plot, "b13_combined_ratio_plot", width = 14, height = 8)
save_plot(tree_size_comparison, "b14_tree_size_comparison", width = 12, height = 8)
save_plot(side_by_side_plot, "b15_side_by_side_plot", width = 14, height = 8)
save_plot(detailed_histograms, "b16_detailed_histograms", width = 16, height = 12)
save_plot(size_vs_type_effect, "b17_size_vs_type_effect", width = 16, height = 12)
save_plot(tree_structure_influence, "b18_tree_structure_influence", width = 14, height = 10)
save_plot(node_size_effectiveness, "b19_node_size_effectiveness", width = 14, height = 10)
save_plot(tradeoff_plot, "b20_tradeoff_plot", width = 14, height = 10)

# Uložení grafů ze sady C a D
save_plot(ratio_comparison_plot, "c01_ratio_comparison_plot", width = 14, height = 10)
save_plot(size_category_box, "c02_size_category_box", width = 14, height = 10)
save_plot(spider_chart, "c03_spider_chart", width = 12, height = 10)
save_plot(pareto_plot, "c04_pareto_plot", width = 14, height = 10)
save_plot(heatmap_plot, "c05_heatmap_plot", width = 12, height = 8)
save_plot(cdf_plot, "c06_cdf_plot", width = 14, height = 10)
save_plot(improvement_plot, "c07_improvement_plot", width = 16, height = 10)
save_plot(tradeoff_space_time, "c08_tradeoff_space_time", width = 14, height = 10)
save_plot(optimization_plot, "c09_optimization_plot", width = 16, height = 10)
save_plot(ranking_plot, "d01_ranking_plot", width = 12, height = 8)
save_plot(breakdown_plot, "d02_breakdown_plot", width = 12, height = 8)
save_plot(avg_median_plot, "d03_avg_median_plot", width = 16, height = 10)
save_plot(density_with_quantiles, "d04_density_with_quantiles", width = 14, height = 10)

# ================================================================
# VYTVOŘENÍ NOVÝCH DASHBOARDŮ
# ================================================================

# Dashboard pro porovnání interakcí a efektů
dashboard_interactions <- (method_opt_interaction / size_vs_type_effect)

# Dashboard pro celkové hodnocení metod
dashboard_evaluation <- (scenario_heatmap | ranking_plot) /
                       (breakdown_plot | spider_chart)

# Dashboard pro analýzu distribucí
dashboard_distributions <- (detailed_histograms / density_with_quantiles) |
                          (combined_ratio_plot / cdf_plot)

# Dashboard pro analýzu trade-offs
dashboard_tradeoffs <- (tradeoff_plot / tradeoff_space_time) |
                      (combined_plot / pareto_plot)

# Uložení nových dashboardů
save_plot(dashboard_interactions, "dashboard_9_interactions", width = 16, height = 16)
save_plot(dashboard_evaluation, "dashboard_10_evaluation", width = 16, height = 16)
save_plot(dashboard_distributions, "dashboard_11_distributions", width = 18, height = 16)
save_plot(dashboard_tradeoffs, "dashboard_12_tradeoffs", width = 16, height = 16)

# ================================================================
# ZÁVĚREČNÉ SHRNUTÍ
# ================================================================

cat("\n\n=================================================================\n")
cat("ANALÝZA KOMPRESE STROMOVÝCH STRUKTUR - KOMPLETNÍ SADA VIZUALIZACÍ\n")
cat("=================================================================\n\n")

cat("Celkový počet vygenerovaných grafů:", 
    21 + 20 + 10 + 4, "\n")

cat("Celkový počet dashboardů:", 
    4 + 4 + 4, "\n")

cat("\nVšechny grafy byly úspěšně uloženy do adresáře 'visualization'.\n")
cat("Grafy jsou rozděleny do kategorií podle typu analýz:\n")
cat(" - Základní analýzy (01-21)\n")
cat(" - Rozšířené analýzy (a01-a20)\n")
cat(" - Pokročilé vizualizace (b01-b20)\n")
cat(" - Komplexní analýzy a hodnocení (c01-c09, d01-d04)\n\n")

cat("Dashboardy kombinují související grafy pro komplexní pohled na data.\n")
cat(" - Dashboardy 1-4: Základní porovnání kompresních metod\n")
cat(" - Dashboardy 5-8: Rozšířené analýzy podle různých faktorů\n")
cat(" - Dashboardy 9-12: Pokročilé multifaktorové analýzy a hodnocení\n\n")

cat("Hlavní poznatky z analýzy lze najít v následujících grafech:\n")
cat(" - Celkové hodnocení metod: d01_ranking_plot, c03_spider_chart\n")
cat(" - Optimální metody pro různé scénáře: b10_scenario_heatmap\n")
cat(" - Trade-off mezi kompresním poměrem a rychlostí: c04_pareto_plot\n")
cat(" - Efektivita podle typu souborů: b03_matrix_plot, c05_heatmap_plot\n\n")

cat("Analýza úspěšně dokončena!\n")