# Kompresní poměr vzhledem k velikosti dat ####

install.packages("showtext")
library(ggplot2)
library(showtext)

# Aktivování showtext pro podporu českých znaků
showtext_auto()

# Základní styl pro všechny grafy
base_plot <- function(data, title_text) {
  ggplot(data, aes(x = Size, y = CompressionRatio)) +
    geom_point(color = "#0073C2FF", size = 2.5) +
    geom_hline(yintercept = 1.0, linetype = "dashed", color = "darkred", size = 1) +
    scale_x_log10(labels = scales::comma) +
    scale_y_continuous(
      breaks = seq(0, max(data$CompressionRatio, na.rm = TRUE), by = 0.1),
      labels = scales::number_format(scale = 1)
    ) +
    labs(
      title = title_text,
      subtitle = "Přerušovaná čára značí poměr komprese = 1.0\nHodnoty pod čarou jsou komprimované, nad čarou jsou větší než originál.",
      x = "Velikost (logaritmická stupnice) v Bytech",
      y = "Poměr komprese"
    ) +
    theme_minimal(base_size = 14) +
    theme(
      plot.title = element_text(face = "bold"),
      plot.subtitle = element_text(size = 11),
      axis.title = element_text(face = "bold"),
      panel.grid.minor = element_blank()
    )
}

base_plot(report, "Kompresní poměr vzhledem k velikosti dat")

report$Type
# Kompresní poměr vzhledem k velikosti dat technické dokumentace ####
report_tech <- subset(report, Type == "/Users/marekberan/Code/SP/tree-compression/src/ConsoleApp/bin/Debug/net9.0/Resources/Texts/technical_docs/kernel_docs")
base_plot(report_tech, "Kompresní poměr vzhledem k velikosti technické dokumentace")

# Kompresní poměr vzhledem k velikosti dat prózaická data ####
report_proza <- subset(report, Type == "/Users/marekberan/Code/SP/tree-compression/src/ConsoleApp/bin/Debug/net9.0/Resources/Texts/prose")
base_plot(report_proza, "Kompresní poměr vzhledem k velikosti prózaických dat")

# Kompresní poměr vzhledem k velikosti dat právní dokumenty ####
report_pravo <- subset(report, Type == "/Users/marekberan/Code/SP/tree-compression/src/ConsoleApp/bin/Debug/net9.0/Resources/Texts/legal_papers")
base_plot(report_pravo, "Kompresní poměr vzhledem k velikosti právních dokumentů")


report


library(ggplot2)
library(dplyr)
library(reshape2)
library(stringr)

# Truncate the Type column to only show the last 3 characters
report$Type <- str_sub(report$Type, -10)

# Aggregate data by Type (averaging numeric columns)
aggregated_data <- report %>%
  group_by(Type) %>%
  summarise(
    AvgCompressionTime = mean(as.numeric(CompressionTime), na.rm = TRUE),
    AvgDecompressionTime = mean(as.numeric(DecompressionTime), na.rm = TRUE),
    AvgCompressionRatio = mean(CompressionRatio, na.rm = TRUE)
  )

# Reshape the data to long format for easier plotting
aggregated_data_long <- melt(aggregated_data, id.vars = "Type")

# Plot heatmap
ggplot(aggregated_data_long, aes(x = variable, y = Type, fill = value)) +
  geom_tile() +
  scale_fill_gradient(low = "white", high = "blue") +
  theme_minimal() +
  labs(
    title = "Teplotná mapa kompresních a dekompresních metrik podle typu souboru",
    x = "Metrika",
    y = "Typ souboru",
    fill = "Hodnota"
  )


library(dplyr)

aggregated_data <- aggregated_data %>%
  mutate(Type = recode(Type,
                       "ernel_docs" = "Technická dokumentace",
                       "exts/prose" = "Próza",
                       "gal_papers" = "Právní dokumenty",
                    
                       # Add other mappings as needed
  ))



# Bar plot of average compression ratio by file type
ggplot(aggregated_data, aes(x = Type, y = AvgCompressionRatio, fill = Type)) +
  geom_bar(stat = "identity") +
  theme_minimal() +
  labs(
    title = "Průměrný kompresní poměr podle typu souboru",
    x = "Typ textu",
    y = "Průměrný kompresní poměr"
  )




# Find the file name with the minimum compression ratio
min_compression_ratio_file <- report$FileName[which.min(report$CompressionRatio)]

# Display the result
min_compression_ratio_file





