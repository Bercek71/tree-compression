
# Kompresní poměr vzhledem k velikosti dat  ####

install.packages("showtext")
library(ggplot2)

# Aktivování showtext pro podporu českých znaků
showtext_auto()

# Vykreslení scatter plotu s požadovanými úpravami
ggplot(report, aes(x = Size, y = CompressionRatio)) +
  geom_point(color = "#0073C2FF", size = 2.5) +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
  scale_x_log10(labels = scales::comma) +
  scale_y_continuous(
    breaks = seq(0, max(report$CompressionRatio, na.rm = TRUE), by = 0.1),  # Desítkové hodnoty
    labels = scales::number_format(scale = 1)  # Formátování čísel bez vědecké notace
  ) +
  labs(
    title = "Kompresní poměr vzhledem k velikosti dat",
    subtitle = "Přerušovaná čára značí poměr komprese = 1.0\nHodnoty pod čarou jsou komprimované, nad čarou jsou větší než originál.",
    x = "Velikost (logaritmická stupnice)",
    y = "Poměr komprese"
  ) +
  theme_minimal(base_size = 14) +
  theme(
    plot.title = element_text(face = "bold"),
    plot.subtitle = element_text(size = 11),
    axis.title = element_text(face = "bold"),
    panel.grid.minor = element_blank()
  )
