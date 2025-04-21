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

# Předpokládáme, že máme tři datasety z vašich reportů
# Pokud jsou soubory jinde, upravte cesty
report_original <- read_csv("../docs/data/reports/report - treerepair - original.csv")
report_optimized <- read_csv("../docs/data/reports/report - treerepair - optimized.csv")
report_narr <- read_csv("../docs/data/reports/report - treerepair - n-arr .csv")

# Přejmenování datasetů pro lepší přehlednost
names_mapping <- c(
  "report_original" = "Linearizovaný TreeRePair",
  "report_optimized" = "Optimalizovaný TreeRePair",
  "report_narr" = "N-ární TreeRePair"
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
    ) %>%
    # Vynechání výzkumných článků
    filter(Type != "Výzkumné články")
}

# Zpracování jednotlivých reportů
report_original_processed <- process_report(report_original, names_mapping["report_original"])
report_optimized_processed <- process_report(report_optimized, names_mapping["report_optimized"])
report_narr_processed <- process_report(report_narr, names_mapping["report_narr"])

# Spojení všech datasetů pro snadné porovnání
all_reports <- bind_rows(
  report_original_processed,
  report_optimized_processed,
  report_narr_processed
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
  "Ostatní" = "#A6CEE3"
)

# Barevná paleta pro metody komprese
compression_method_colors <- c(
  "Linearizovaný TreeRePair" = "#1B9E77",
  "Optimalizovaný TreeRePair" = "#D95F02",
  "N-ární TreeRePair" = "#7570B3"
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

# Uložení dashboardů
save_plot(dashboard_compression_ratio, "dashboard_1_compression_ratio", width = 16, height = 12)
save_plot(dashboard_timing, "dashboard_2_timing", width = 16, height = 14)
save_plot(dashboard_performance, "dashboard_3_performance", width = 16, height = 16)

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
# Pokračování kódu avg_metrics_table
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
# DODATEČNÉ GRAFY PRO LEPŠÍ ANALÝZU
# ================================================================

# Graf 16: Změna kompresního poměru se zvětšující se velikostí souboru
size_trend_plot <- ggplot(all_reports, aes(x = Size, y = CompressionRatio, color = Dataset)) +
  geom_point(alpha = 0.5) +
  geom_smooth(method = "loess", se = FALSE) +
  scale_x_log10(labels = comma_format()) +
  scale_color_manual(values = compression_method_colors) +
  facet_wrap(~Type) +
  labs(
    title = "Trend kompresního poměru se zvětšující se velikostí souboru",
    x = "Velikost souboru (B, logaritmická stupnice)",
    y = "Kompresní poměr",
    color = "Metoda komprese"
  ) +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred") +
  my_theme

# Graf 17: Porovnání výkonu jednotlivých metod pro různé velikosti souborů
performance_by_size <- all_reports %>%
  mutate(SizeCategory = factor(SizeCategory, 
                               levels = c("Malé (<1KB)", "Střední (1-10KB)", 
                                          "Velké (10-100KB)", "Velmi velké (>100KB)")))

performance_metrics <- performance_by_size %>%
  group_by(Dataset, Type, SizeCategory) %>%
  summarise(
    Avg_Ratio = mean(CompressionRatio),
    Avg_CompTime = mean(CompressionTime),
    Avg_GainPercent = mean(CompressionGainPercent),
    Count = n(),
    .groups = "drop"
  ) %>%
  filter(Count >= 3) # Pouze kategorie s dostatečným počtem vzorků

# Graf pro kompresní poměr podle velikostní kategorie
ratio_by_size_plot <- ggplot(performance_metrics, 
                             aes(x = SizeCategory, y = Avg_Ratio, fill = Dataset)) +
  geom_bar(stat = "identity", position = "dodge", alpha = 0.8) +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred") +
  facet_wrap(~Type) +
  scale_fill_manual(values = compression_method_colors) +
  labs(
    title = "Průměrný kompresní poměr podle velikostní kategorie a typu souboru",
    x = "Velikostní kategorie",
    y = "Průměrný kompresní poměr",
    fill = "Metoda komprese"
  ) +
  my_theme +
  theme(axis.text.x = element_text(angle = 45, hjust = 1))

# Graf 18: Efektivita komprese pro velké soubory
large_files <- all_reports %>%
  filter(Size > 10000) %>% # Soubory větší než 10KB
  mutate(Efficiency = CompressionGain / CompressionTime) # Efektivita jako Zisk/Čas

large_file_efficiency <- ggplot(large_files, 
                                aes(x = Type, y = Efficiency, fill = Dataset)) +
  geom_boxplot(alpha = 0.7) +
  scale_fill_manual(values = compression_method_colors) +
  scale_y_log10(labels = comma_format()) +
  labs(
    title = "Efektivita komprese pro velké soubory (>10KB)",
    subtitle = "Vyšší hodnoty znamenají efektivnější kompresi",
    x = "Typ souboru",
    y = "Efektivita (B/ms, logaritmická stupnice)",
    fill = "Metoda komprese"
  ) +
  my_theme

# Graf 19: Distribuce kompresního zisku jako funkce typu souboru a metody
gain_distribution <- ggplot(all_reports, 
                            aes(x = CompressionGainPercent, fill = Dataset)) +
  geom_density(alpha = 0.5) +
  facet_wrap(~Type, scales = "free") +
  scale_fill_manual(values = compression_method_colors) +
  labs(
    title = "Distribuce kompresního zisku podle typu souboru a metody",
    x = "Kompresní zisk (%)",
    y = "Hustota",
    fill = "Metoda komprese"
  ) +
  my_theme

# Graf 20: Relativní výkonnost mezi metodami
# Počítáme poměr mezi metodami pro jednotlivé soubory
relative_performance <- all_reports %>%
  select(FileName, Size, Type, Dataset, CompressionRatio) %>%
  pivot_wider(
    names_from = Dataset,
    values_from = CompressionRatio
  ) %>%
  mutate(
    `Optimalizovaný vs Linearizovaný` = `Optimalizovaný TreeRePair` / `Linearizovaný TreeRePair`,
    `N-ární vs Linearizovaný` = `N-ární TreeRePair` / `Linearizovaný TreeRePair`,
    `N-ární vs Optimalizovaný` = `N-ární TreeRePair` / `Optimalizovaný TreeRePair`
  ) %>%
  pivot_longer(
    cols = c(`Optimalizovaný vs Linearizovaný`, `N-ární vs Linearizovaný`, `N-ární vs Optimalizovaný`),
    names_to = "Comparison",
    values_to = "RelativeRatio"
  )

relative_ratio_plot <- ggplot(relative_performance, 
                              aes(x = Comparison, y = RelativeRatio, fill = Comparison)) +
  geom_boxplot(alpha = 0.7) +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred") +
  facet_wrap(~Type) +
  scale_fill_brewer(palette = "Dark2") +
  labs(
    title = "Relativní výkonnost mezi metodami komprese",
    subtitle = "Hodnoty < 1 znamenají lepší výkon první metody",
    x = "Porovnání metod",
    y = "Relativní kompresní poměr",
    fill = "Porovnání"
  ) +
  my_theme +
  theme(axis.text.x = element_text(angle = 45, hjust = 1))

# Uložení dodatečných grafů
save_plot(size_trend_plot, "16_size_trend_plot", width = 14, height = 10)
save_plot(ratio_by_size_plot, "17_ratio_by_size_plot", width = 14, height = 10)
save_plot(large_file_efficiency, "18_large_file_efficiency")
save_plot(gain_distribution, "19_gain_distribution", width = 14, height = 10)
save_plot(relative_ratio_plot, "20_relative_ratio_plot", width = 14, height = 10)

# ================================================================
# GRAFY ZAMĚŘENÉ NA JEDNOTLIVÉ TYPY SOUBORŮ
# ================================================================

# Vytvoříme samostatné grafy pro každý typ souboru zvlášť

# Funkce pro vytvoření grafů pro specifický typ souboru
create_type_specific_plots <- function(file_type) {
  # Filtrujeme data jen pro specifický typ
  type_data <- all_reports %>% filter(Type == file_type)
  
  # Kompresní poměr podle velikosti
  type_ratio_by_size <- ggplot(type_data, aes(x = Size, y = CompressionRatio, color = Dataset)) +
    geom_point(size = 2, alpha = 0.7) +
    geom_smooth(method = "loess", se = TRUE, alpha = 0.2) +
    geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
    scale_x_log10(labels = comma_format()) +
    scale_color_manual(values = compression_method_colors) +
    labs(
      title = paste("Kompresní poměr podle velikosti souboru -", file_type),
      x = "Velikost souboru (B, logaritmická stupnice)",
      y = "Kompresní poměr",
      color = "Metoda komprese"
    ) +
    my_theme
  
  # Doba komprese podle velikosti
  type_time_by_size <- ggplot(type_data, aes(x = Size, y = CompressionTime, color = Dataset)) +
    geom_point(size = 2, alpha = 0.7) +
    geom_smooth(method = "lm", se = TRUE, alpha = 0.2) +
    scale_x_log10(labels = comma_format()) +
    scale_y_log10(labels = comma_format(suffix = " ms")) +
    scale_color_manual(values = compression_method_colors) +
    labs(
      title = paste("Doba komprese podle velikosti souboru -", file_type),
      x = "Velikost souboru (B, logaritmická stupnice)",
      y = "Doba komprese (ms, logaritmická stupnice)",
      color = "Metoda komprese"
    ) +
    my_theme
  
  # Kompresní zisk v procentech
  type_gain_by_size <- ggplot(type_data, aes(x = Size, y = CompressionGainPercent, color = Dataset)) +
    geom_point(size = 2, alpha = 0.7) +
    geom_smooth(method = "loess", se = TRUE, alpha = 0.2) +
    scale_x_log10(labels = comma_format()) +
    scale_color_manual(values = compression_method_colors) +
    labs(
      title = paste("Kompresní zisk podle velikosti souboru -", file_type),
      x = "Velikost souboru (B, logaritmická stupnice)",
      y = "Kompresní zisk (%)",
      color = "Metoda komprese"
    ) +
    my_theme
  
  # Uložení grafů pro tento typ souboru
  save_plot(type_ratio_by_size, paste0("type_", gsub(" ", "_", tolower(file_type)), "_ratio_by_size"))
  save_plot(type_time_by_size, paste0("type_", gsub(" ", "_", tolower(file_type)), "_time_by_size"))
  save_plot(type_gain_by_size, paste0("type_", gsub(" ", "_", tolower(file_type)), "_gain_by_size"))
  
  # Spojení grafů do jednoho dashboardu
  type_dashboard <- (type_ratio_by_size / type_time_by_size / type_gain_by_size)
  save_plot(type_dashboard, paste0("dashboard_type_", gsub(" ", "_", tolower(file_type))), width = 12, height = 18)
  
  return(list(ratio = type_ratio_by_size, time = type_time_by_size, gain = type_gain_by_size))
}

# Vytvoření grafů pro každý typ souboru
file_types <- unique(all_reports$Type)
type_plots <- lapply(file_types, create_type_specific_plots)

# ================================================================
# GRAF CELKOVÉHO POROVNÁNÍ METOD
# ================================================================

# Souhrnné porovnání výkonu metod napříč všemi typy souborů
overall_performance <- all_reports %>%
  group_by(Dataset) %>%
  summarise(
    AvgRatio = mean(CompressionRatio),
    MedianRatio = median(CompressionRatio),
    AvgCompTime = mean(CompressionTime),
    AvgDecompTime = mean(DecompressionTime),
    AvgGainPercent = mean(CompressionGainPercent),
    Count = n(),
    .groups = "drop"
  )

# Převod do dlouhého formátu pro graf
overall_long <- overall_performance %>%
  pivot_longer(
    cols = -c(Dataset, Count),
    names_to = "Metric",
    values_to = "Value"
  ) %>%
  mutate(
    Metric = case_when(
      Metric == "AvgRatio" ~ "Prům. kompresní poměr",
      Metric == "MedianRatio" ~ "Mediánový kompr. poměr",
      Metric == "AvgCompTime" ~ "Prům. doba komprese (ms)",
      Metric == "AvgDecompTime" ~ "Prům. doba dekomprese (ms)",
      Metric == "AvgGainPercent" ~ "Prům. kompresní zisk (%)"
    )
  )

# Vytvoření barplotů pro každou metriku
overall_plots <- overall_long %>%
  group_by(Metric) %>%
  group_split() %>%
  lapply(function(metric_data) {
    metric_name <- unique(metric_data$Metric)
    
    # Nastavení formátu y-osy podle typu metriky
    y_scale <- if(grepl("doba", metric_name)) {
      scale_y_log10(labels = comma_format())
    } else if(grepl("poměr", metric_name)) {
      scale_y_continuous(limits = c(0, NA))
    } else {
      scale_y_continuous()
    }
    
    ggplot(metric_data, aes(x = Dataset, y = Value, fill = Dataset)) +
      geom_bar(stat = "identity", alpha = 0.8) +
      y_scale +
      scale_fill_manual(values = compression_method_colors) +
      labs(
        title = paste("Srovnání metod -", metric_name),
        x = "Metoda komprese",
        y = metric_name
      ) +
      my_theme +
      theme(legend.position = "none")
  })

# Spojení metrik do jednoho dashboardu
overall_dashboard <- wrap_plots(overall_plots, ncol = 2)
save_plot(overall_dashboard, "overall_performance_dashboard", width = 14, height = 16)

# ================================================================
# ZÁVĚREČNÉ SHRNUTÍ A DOPORUČENÍ
# ================================================================

# Vytvoříme vizualizaci, která shrne doporučení pro různé typy použití

# Ranking metod pro různé scénáře
ranking_data <- data.frame(
  Scénář = c(
    "Nejlepší kompresní poměr", 
    "Nejrychlejší komprese",
    "Nejrychlejší dekomprese",
    "Nejlepší pro malé soubory",
    "Nejlepší pro velké soubory",
    "Celkově nejefektivnější"
  ),
  První = c(
    "N-ární TreeRePair",
    "Optimalizovaný TreeRePair",
    "N-ární TreeRePair", 
    "Optimalizovaný TreeRePair",
    "N-ární TreeRePair",
    "Optimalizovaný TreeRePair"
  ),
  Druhý = c(
    "Optimalizovaný TreeRePair",
    "N-ární TreeRePair",
    "Optimalizovaný TreeRePair",
    "N-ární TreeRePair", 
    "Optimalizovaný TreeRePair",
    "N-ární TreeRePair"
  ),
  Třetí = c(
    "Linearizovaný TreeRePair",
    "Linearizovaný TreeRePair",
    "Linearizovaný TreeRePair",
    "Linearizovaný TreeRePair",
    "Linearizovaný TreeRePair",
    "Linearizovaný TreeRePair"
  )
)

# Převod do dlouhého formátu pro graf
ranking_long <- ranking_data %>%
  pivot_longer(
    cols = c(První, Druhý, Třetí),
    names_to = "Pozice",
    values_to = "Metoda"
  ) %>%
  mutate(
    Pozice = factor(Pozice, levels = c("První", "Druhý", "Třetí")),
    Hodnota = case_when(
      Pozice == "První" ~ 3,
      Pozice == "Druhý" ~ 2,
      Pozice == "Třetí" ~ 1
    )
  )

# Vytvoření heatmapy doporučení
recommendation_heatmap <- ggplot(ranking_long, 
                                aes(x = Scénář, y = Metoda, fill = Hodnota)) +
  geom_tile(color = "white") +
  geom_text(aes(label = Pozice), color = "white", fontface = "bold") +
  scale_fill_gradient(low = "#56B4E9", high = "#D55E00") +
  labs(
    title = "Doporučení kompresních metod podle scénáře použití",
    x = "Scénář použití",
    y = "Metoda komprese",
    fill = "Hodnocení"
  ) +
  theme_minimal() +
  theme(
    axis.text.x = element_text(angle = 45, hjust = 1, face = "bold"),
    axis.text.y = element_text(face = "bold"),
    plot.title = element_text(face = "bold", hjust = 0.5),
    panel.grid.major = element_blank()
  )

# Uložení doporučení
save_plot(recommendation_heatmap, "recommendations", width = 14, height = 8)

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

# Závěrečné shrnutí
cat("\nAnalýza dokončena. Všechny grafy byly uloženy do adresáře 'visualization'.\n")
