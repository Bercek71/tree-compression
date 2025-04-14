# ================================================================
# KOMPRESNÍ ANALÝZA - PŘEPRACOVANÝ KÓD ####
# ================================================================

# Instalace a načtení potřebných knihoven
if (!require("showtext")) install.packages("showtext")
if (!require("ggplot2")) install.packages("ggplot2")
if (!require("dplyr")) install.packages("dplyr")
if (!require("tidyr")) install.packages("tidyr")
if (!require("scales")) install.packages("scales")
if (!require("patchwork")) install.packages("patchwork")
if (!require("viridis")) install.packages("viridis")

library(showtext)
library(ggplot2)
library(dplyr)
library(tidyr)
library(scales)
library(patchwork)
library(viridis)

# Aktivace podpory českých znaků
showtext_auto()

# ================================================================
# PŘÍPRAVA DAT
# ================================================================

# Předpokládáme, že máme dva datasety: report a report_optimized
# Pokud potřebujete použít optimalizovanou verzi, odkomentujte následující řádek
# report <- report_optimized

# Úprava názvů typů souborů pro lepší čitelnost
report <- report %>%
  mutate(Type = case_when(
    grepl("kernel_docs", Type) ~ "Technická dokumentace",
    grepl("prose", Type) ~ "Próza",
    grepl("legal_papers", Type) ~ "Právní dokumenty",
    TRUE ~ "Ostatní"
  )) %>%
  # Převod časů na milisekundy pro lepší interpretaci
  mutate(
    CompressionTime = as.numeric(CompressionTime) * 1000,
    DecompressionTime = as.numeric(DecompressionTime) * 1000,
    # Výpočet efektivity komprese/dekomprese (ms / % změny velikosti)
    CompressionEfficiency = CompressionTime / (abs(1 - CompressionRatio) * 100),
    DecompressionEfficiency = DecompressionTime / (abs(1 - CompressionRatio) * 100),
    # Výpočet rychlosti komprese (B/ms)
    CompressionSpeed = Size / CompressionTime,
    DecompressionSpeed = CompressedSize / DecompressionTime,
    # Kompresní zisk v bytech
    CompressionGain = Size - CompressedSize
  )

# Vytvoření dlouhého formátu dat pro některé grafy
report_long <- report %>%
  select(Type, FileName, Size, CompressionTime, DecompressionTime, CompressionRatio) %>%
  pivot_longer(cols = c(CompressionTime, DecompressionTime), 
               names_to = "Fáze", values_to = "Čas")

report_efficiency_long <- report %>%
  select(Type, FileName, Size, CompressionEfficiency, DecompressionEfficiency) %>%
  pivot_longer(cols = c(CompressionEfficiency, DecompressionEfficiency),
               names_to = "Fáze", values_to = "Efektivita")

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
file_type_colors <- c("Technická dokumentace" = "#2C7FB8", 
                      "Próza" = "#7FBC41", 
                      "Právní dokumenty" = "#D7301F")

# ================================================================
# GRAF 1: KOMPRESNÍ POMĚR VZHLEDEM K VELIKOSTI DAT
# ================================================================

plot_compression_ratio <- function(data, title = "Kompresní poměr vzhledem k velikosti dat") {
  ggplot(data, aes(x = Size, y = CompressionRatio, color = Type)) +
    geom_point(size = 2.5, alpha = 0.7) +
    geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
    scale_x_log10(labels = comma_format()) +
    scale_y_continuous(
      breaks = seq(0, max(data$CompressionRatio, na.rm = TRUE) + 0.1, by = 0.1),
      labels = number_format(accuracy = 0.01)
    ) +
    scale_color_manual(values = file_type_colors) +
    labs(
      title = title,
      subtitle = "Přerušovaná čára značí poměr komprese = 1.0\nHodnoty pod čarou jsou komprimované, nad čarou jsou větší než originál",
      x = "Velikost souboru (B, logaritmická stupnice)",
      y = "Poměr komprese",
      color = "Typ souboru"
    ) +
    my_theme
}

p1 <- plot_compression_ratio(report)

# ================================================================
# GRAF 2: DOBA KOMPRESE VS VELIKOST SOUBORU
# ================================================================

p2 <- ggplot(report, aes(x = Size, y = CompressionTime, color = Type)) +
  geom_point(size = 2.5, alpha = 0.7) +
  geom_smooth(method = "lm", se = FALSE, linetype = "dashed", alpha = 0.5) +
  scale_x_log10(labels = comma_format()) +
  scale_y_log10(labels = comma_format(suffix = " ms")) +
  scale_color_manual(values = file_type_colors) +
  labs(
    title = "Doba komprese vs velikost souboru",
    x = "Velikost souboru (B, logaritmická stupnice)",
    y = "Doba komprese (ms, logaritmická stupnice)",
    color = "Typ souboru"
  ) +
  my_theme

# ================================================================
# GRAF 3: POROVNÁNÍ DOBY KOMPRESE A DEKOMPRESE
# ================================================================

p3 <- ggplot(report_long, aes(x = Size, y = Čas, color = Fáze)) +
  geom_point(alpha = 0.7) +
  geom_smooth(method = "lm", se = FALSE, linetype = "dashed") +
  scale_x_log10(labels = comma_format()) +
  scale_y_log10(labels = comma_format(suffix = " ms")) +
  facet_wrap(~Type, scales = "free") +
  labs(
    title = "Porovnání doby komprese a dekomprese podle typu souboru",
    x = "Velikost souboru (B, logaritmická stupnice)",
    y = "Doba zpracování (ms, logaritmická stupnice)",
    color = "Operace"
  ) +
  scale_color_brewer(palette = "Set1", 
                     labels = c("CompressionTime" = "Komprese", 
                                "DecompressionTime" = "Dekomprese")) +
  my_theme

# ================================================================
# GRAF 4: BOXPLOTY KOMPRESNÍHO POMĚRU PODLE TYPU
# ================================================================

p4 <- ggplot(report, aes(x = Type, y = CompressionRatio, fill = Type)) +
  geom_boxplot(alpha = 0.7) +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  scale_fill_manual(values = file_type_colors) +
  labs(
    title = "Rozložení kompresního poměru podle typu souboru",
    x = "Typ souboru",
    y = "Kompresní poměr",
    fill = "Typ souboru"
  ) +
  my_theme +
  theme(legend.position = "none")

# ================================================================
# GRAF 5: EFEKTIVITA KOMPRESE A DEKOMPRESE
# ================================================================

p5 <- ggplot(report_efficiency_long, aes(x = Type, y = Efektivita, fill = Fáze)) +
  geom_boxplot(alpha = 0.7, position = "dodge") +
  scale_y_log10(labels = comma_format(suffix = " ms")) +
  scale_fill_brewer(palette = "Set1", 
                    labels = c("CompressionEfficiency" = "Komprese", 
                               "DecompressionEfficiency" = "Dekomprese")) +
  labs(
    title = "Efektivita komprese a dekomprese podle typu textu",
    x = "Typ textu",
    y = "Efektivita (ms na změnu 1% velikosti, log)",
    fill = "Operace"
  ) +
  my_theme

# ================================================================
# GRAF 6: HISTOGRAM KOMPRESNÍHO POMĚRU
# ================================================================

p6 <- ggplot(report, aes(x = CompressionRatio, fill = Type)) +
  geom_histogram(binwidth = 0.05, alpha = 0.7, color = "black") +
  scale_fill_manual(values = file_type_colors) +
  labs(
    title = "Rozdělení kompresního poměru podle typu souboru",
    x = "Kompresní poměr",
    y = "Počet souborů",
    fill = "Typ souboru"
  ) +
  my_theme

# ================================================================
# GRAF 7: KOMPRESNÍ ZISK VS KOMPRESNÍ ČAS
# ================================================================

p7 <- ggplot(report, aes(x = CompressionTime, y = CompressionGain, color = Type)) +
  geom_point(size = 2.5, alpha = 0.7) +
  scale_x_log10(labels = comma_format(suffix = " ms")) +
  scale_y_log10(labels = comma_format(suffix = " B")) +
  scale_color_manual(values = file_type_colors) +
  labs(
    title = "Kompresní zisk vs doba komprese",
    x = "Doba komprese (ms, logaritmická stupnice)",
    y = "Kompresní zisk (B, logaritmická stupnice)",
    color = "Typ souboru"
  ) +
  my_theme

# ================================================================
# GRAF 8: RYCHLOST KOMPRESE A DEKOMPRESE
# ================================================================

report_speed_long <- report %>%
  select(Type, FileName, CompressionSpeed, DecompressionSpeed) %>%
  pivot_longer(cols = c(CompressionSpeed, DecompressionSpeed),
               names_to = "Operace", values_to = "Rychlost")

p8 <- ggplot(report_speed_long, aes(x = Type, y = Rychlost, fill = Operace)) +
  geom_boxplot(alpha = 0.7) +
  scale_y_log10(labels = comma_format(suffix = " B/ms")) +
  scale_fill_brewer(palette = "Set1", 
                    labels = c("CompressionSpeed" = "Komprese", 
                               "DecompressionSpeed" = "Dekomprese")) +
  labs(
    title = "Rychlost komprese a dekomprese podle typu souboru",
    x = "Typ souboru",
    y = "Rychlost (B/ms, logaritmická stupnice)",
    fill = "Operace"
  ) +
  my_theme

# ================================================================
# POROVNÁNÍ REPORT A REPORT_OPTIMIZED (pokud existují)
# ================================================================

# Pokud máme k dispozici oba datasety, můžeme je porovnat
if (exists("report_optimized")) {
  # Příprava dat report_optimized stejným způsobem jako report
  report_optimized <- report_optimized %>%
    mutate(Type = case_when(
      grepl("kernel_docs", Type) ~ "Technická dokumentace",
      grepl("prose", Type) ~ "Próza",
      grepl("legal_papers", Type) ~ "Právní dokumenty",
      TRUE ~ NA_character_
    )) %>%
    filter(!is.na(Type)) %>%
    mutate(
      CompressionTime = as.numeric(CompressionTime) * 1000,
      DecompressionTime = as.numeric(DecompressionTime) * 1000
    )
  
  # Sloučení datasetů pro porovnání
  comparison_data <- bind_rows(
    mutate(report, Dataset = "Původní"),
    mutate(report_optimized, Dataset = "Optimalizovaný")
  )
  
  # Graf porovnání kompresního poměru
  p9 <- ggplot(comparison_data, aes(x = Type, y = CompressionRatio, fill = Dataset)) +
    geom_boxplot(alpha = 0.7, position = "dodge") +
    geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
    labs(
      title = "Porovnání kompresního poměru: původní vs optimalizovaný",
      x = "Typ souboru",
      y = "Kompresní poměr",
      fill = "Dataset"
    ) +
    my_theme
  
  # Graf porovnání kompresního času
  p10 <- ggplot(comparison_data, aes(x = Type, y = CompressionTime, fill = Dataset)) +
    geom_boxplot(alpha = 0.7, position = "dodge") +
    scale_y_log10(labels = comma_format(suffix = " ms")) +
    labs(
      title = "Porovnání doby komprese: původní vs optimalizovaný",
      x = "Typ souboru",
      y = "Doba komprese (ms, logaritmická stupnice)",
      fill = "Dataset"
    ) +
    my_theme
  
  # Graf porovnání dekompresního času
  p11 <- ggplot(comparison_data, aes(x = Type, y = DecompressionTime, fill = Dataset)) +
    geom_boxplot(alpha = 0.7, position = "dodge") +
    scale_y_log10(labels = comma_format(suffix = " ms")) +
    labs(
      title = "Porovnání doby dekomprese: původní vs optimalizovaný",
      x = "Typ souboru",
      y = "Doba dekomprese (ms, logaritmická stupnice)",
      fill = "Dataset"
    ) +
    my_theme
  
  # Vytvoření long formátu pro časy komprese a dekomprese
  comparison_time_long <- comparison_data %>%
    select(Type, Dataset, CompressionTime, DecompressionTime) %>%
    pivot_longer(cols = c(CompressionTime, DecompressionTime),
                 names_to = "Operace", values_to = "Čas") %>%
    mutate(Operace = ifelse(Operace == "CompressionTime", "Komprese", "Dekomprese"))
  
  # Graf komplexního porovnání časů komprese a dekomprese
  p12 <- ggplot(comparison_time_long, aes(x = Type, y = Čas, fill = interaction(Dataset, Operace))) +
    geom_boxplot(alpha = 0.7, position = position_dodge(preserve = "single")) +
    scale_y_log10(labels = comma_format(suffix = " ms")) +
    scale_fill_brewer(palette = "Paired", name = "Dataset a operace") +
    labs(
      title = "Komplexní porovnání doby komprese a dekomprese",
      x = "Typ souboru",
      y = "Doba zpracování (ms, logaritmická stupnice)"
    ) +
    my_theme +
    theme(legend.position = "right")
  
  # Výpočet poměru času komprese a dekomprese
  comparison_data <- comparison_data %>%
    mutate(CompressionToDecompressionRatio = CompressionTime / DecompressionTime)
  
  # Graf poměru času komprese k dekompresi
  p13 <- ggplot(comparison_data, aes(x = Type, y = CompressionToDecompressionRatio, fill = Dataset)) +
    geom_boxplot(alpha = 0.7, position = "dodge") +
    geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
    scale_y_log10(labels = comma_format()) +
    labs(
      title = "Poměr doby komprese k dekompresi: původní vs optimalizovaný",
      subtitle = "Hodnoty > 1 znamenají, že komprese trvá déle než dekomprese",
      x = "Typ souboru",
      y = "Poměr komprese/dekomprese (logaritmická stupnice)",
      fill = "Dataset"
    ) +
    my_theme
  
  # Vykreslení porovnávacích grafů
  print(p9)
  print(p10)
  print(p11)
  print(p12)
  print(p13)
  
  # Kombinace grafů do jednoho dashboardu
  comparison_dashboard <- (p9 + p10) / (p11 + p13)
  print(comparison_dashboard)
  
  # Tabulka se základními statistikami porovnání
  stats_comparison <- comparison_data %>%
    group_by(Type, Dataset) %>%
    summarise(
      PrůměrnýKompresníPoměr = mean(CompressionRatio),
      MedianKompresníPoměr = median(CompressionRatio),
      PrůměrnýČasKomprese = mean(CompressionTime),
      PrůměrnýČasDekomprese = mean(DecompressionTime),
      PoměrKompreseDekomprese = mean(CompressionToDecompressionRatio)
    ) %>%
    arrange(Type, Dataset)
  
  print(stats_comparison)
}

# ================================================================
# VYKRESLENÍ GRAFŮ
# ================================================================

# Vykreslení jednotlivých grafů
print(p1)
print(p2)
print(p3)
print(p4)
print(p5)
print(p6)
print(p7)
print(p8)
print(p9)

# Kombinace grafů pro vytvoření dashboardu
# Nahoře: Kompresní poměr a časová analýza
dashboard_top <- (p1 + p2) / p3
print(dashboard_top)

# Uprostřed: Analýza podle typu souborů
dashboard_middle <- (p4 + p6) / p5
print(dashboard_middle)

# Dole: Výkonnostní metriky
dashboard_bottom <- p7 + p8
print(dashboard_bottom)

# ================================================================
# STATISTICKÁ ANALÝZA
# ================================================================

# Základní statistiky podle typu souboru
stats_by_type <- report %>%
  group_by(Type) %>%
  summarise(
    PočetSouborů = n(),
    PrůměrnáVelikost = mean(Size),
    PrůměrnýKompresníPoměr = mean(CompressionRatio),
    MedianKompresníPoměr = median(CompressionRatio),
    PrůměrnýČasKomprese = mean(CompressionTime),
    PrůměrnýČasDekomprese = mean(DecompressionTime),
    PrůměrnáEfektivitaKomprese = mean(CompressionEfficiency, na.rm = TRUE),
    PrůměrnáRychlostKomprese = mean(CompressionSpeed, na.rm = TRUE)
  )

print(stats_by_type)

# Identifikace nejlépe a nejhůře komprimovatelných souborů
best_compressed <- report %>%
  filter(CompressionRatio == min(CompressionRatio)) %>%
  select(FileName, Type, Size, CompressionRatio, CompressionTime)

worst_compressed <- report %>%
  filter(CompressionRatio == max(CompressionRatio)) %>%
  select(FileName, Type, Size, CompressionRatio, CompressionTime)

print("Nejlépe komprimovatelný soubor:")
print(best_compressed)

print("Nejhůře komprimovatelný soubor:")
print(worst_compressed)

