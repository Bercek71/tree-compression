library(ggplot2)

report

ggplot(report, aes(x = Size, y = CompressionRatio)) +
  geom_line(color = "#0073C2FF", size = 1.3) +
  #geom_point(color = "#EFC000FF", size = 3) +
  geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +  # horizontal line
  scale_x_log10(labels = scales::comma) +
  labs(title = "Compression Ratio vs Size",
       subtitle = "Dashed line indicates compression ratio = 1.0",
       x = "Size (log scale)",
       y = "Compression Ratio") +
  theme_minimal(base_size = 14) +
  theme(
    plot.title = element_text(face = "bold"),
    plot.subtitle = element_text(size = 12),
    axis.title = element_text(face = "bold"),
    panel.grid.minor = element_blank()
  )
