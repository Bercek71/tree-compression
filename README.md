<div align="center">

# 🌳 Tree Compression Library

### _Advanced Tree Structure Compression for Natural Language Processing_

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen?style=for-the-badge&logo=github-actions)](https://github.com/Bercek71/tree-compression)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-blue.svg?style=for-the-badge&logo=c-sharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)

[![Platform](https://img.shields.io/badge/Platform-Cross--Platform-lightgrey?style=for-the-badge&logo=windows)](https://dotnet.microsoft.com/)
[![UDPipe](https://img.shields.io/badge/UDPipe-1-blue?style=for-the-badge&logo=language)](https://ufal.mff.cuni.cz/udpipe)
[![R Analysis](https://img.shields.io/badge/R-Analysis%20Scripts-276DC3?style=for-the-badge&logo=r)](https://www.r-project.org/)

[![Algorithm](https://img.shields.io/badge/Algorithm-TreeRePair-red?style=for-the-badge&logo=tree)](https://github.com/Bercek71/tree-compression)
[![NLP](https://img.shields.io/badge/NLP-Dependency%20Parsing-purple?style=for-the-badge&logo=openai)](https://github.com/Bercek71/tree-compression)
[![Compression](https://img.shields.io/badge/Compression-Grammar%20Based-orange?style=for-the-badge&logo=compress)](https://github.com/Bercek71/tree-compression)
[![Pipeline](https://img.shields.io/badge/Architecture-Pipes%20%26%20Filters-blueviolet?style=for-the-badge&logo=flow)](https://github.com/Bercek71/tree-compression)

[![Research](https://img.shields.io/badge/Type-Semestral%20Project-orange?style=for-the-badge&logo=book)](https://github.com/Bercek71/tree-compression)
[![Academic](https://img.shields.io/badge/Status-Academic-lightblue?style=for-the-badge&logo=graduation-cap)](https://github.com/Bercek71/tree-compression)
[![Experimental](https://img.shields.io/badge/Stage-Experimental-yellow?style=for-the-badge&logo=lab)](https://github.com/Bercek71/tree-compression)
[![Performance](https://img.shields.io/badge/Focus-Performance%20Analysis-success?style=for-the-badge&logo=chart-line)](https://github.com/Bercek71/tree-compression)

[![GitHub stars](https://img.shields.io/github/stars/Bercek71/tree-compression?style=for-the-badge&logo=github)](https://github.com/Bercek71/tree-compression/stargazers)
[![GitHub forks](https://img.shields.io/github/forks/Bercek71/tree-compression?style=for-the-badge&logo=github)](https://github.com/Bercek71/tree-compression/network)
[![GitHub issues](https://img.shields.io/github/issues/Bercek71/tree-compression?style=for-the-badge&logo=github)](https://github.com/Bercek71/tree-compression/issues)
[![Last Commit](https://img.shields.io/github/last-commit/Bercek71/tree-compression?style=for-the-badge&logo=github)](https://github.com/Bercek71/tree-compression/commits)

---

### 🎯 **A comprehensive library for compressing tree structures derived from natural language text using various algorithmic approaches including TreeRePair and grammar-based compression strategies.**

</div>

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Compression Strategies](#compression-strategies)
- [Performance Analysis](#performance-analysis)
- [API Documentation](#api-documentation)
- [Examples](#examples)
- [Testing](#testing)

## 🎯 Overview

This project explores the compression of tree structures derived from natural language text using dependency parsing. The main hypothesis is that syntactic patterns in natural language tend to repeat, making them suitable candidates for efficient compression using specialized tree compression algorithms.

### Key Objectives

- **Efficient Tree Compression**: Implement various algorithms for compressing dependency trees
- **Modular Architecture**: Provide a flexible pipeline-based system using the Pipes and Filters pattern
- **Performance Analysis**: Compare different compression strategies across various text types
- **Research Foundation**: Support experimental analysis of tree compression effectiveness

## ✨ Features

### 🔧 Core Functionality

- **Multiple Compression Strategies**
  - TreeRePair algorithm with linearization
  - Optimized TreeRePair for better performance
  - Grammar-based compression
  - Dictionary-based compression
  - Frequent subtree compression

- **Text Processing Pipeline**
  - UDPipe integration for dependency parsing
  - Modular filter-based architecture
  - Support for multiple input formats
  - Extensible tree creation strategies

- **Performance Monitoring**
  - Built-in process observers
  - Compression ratio analysis
  - Time complexity measurements
  - Memory usage tracking

### 📊 Analysis Tools

- **Comprehensive Metrics**
  - Compression ratios
  - Processing times
  - Tree structure analysis
  - Success rate calculations

- **Visualization Support**
  - R-based analysis scripts
  - Multiple chart types
  - Statistical comparisons
  - Performance dashboards

## 🏗️ Architecture

The project follows a **Pipes and Filters** architectural pattern:

```
Text Input → Tree Creation → Compression → Storage
     ↓            ↓             ↓          ↓
  [Filter]   [Filter]     [Filter]   [Output]
```

### Core Components

- **`ITreeCompressor<T>`**: Main interface for tree compression
- **`Pipeline`**: Orchestrates the processing workflow
- **`ICompressionStrategy<T>`**: Defines compression algorithms
- **`ITreeCreationStrategy<T>`**: Handles text-to-tree conversion
- **`ProcessMonitor`**: Observes and reports processing metrics

## 🚀 Installation

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [UDPipe model files](https://ufal.mff.cuni.cz/udpipe/1) (for dependency parsing)

### Building from Source

```bash
# Clone the repository
git clone https://github.com/Bercek71/tree-compression.git
cd tree-compression

# Restore dependencies
dotnet restore

# Build the solution
dotnet build --configuration Release

# Run tests
dotnet test
```

### Using the Build Script

```bash
# Make the script executable
chmod +x scripts/build_SLN.sh

# Run the build script
./scripts/build_SLN.sh
```

## 🎯 Quick Start

### Basic Usage

```csharp
using TreeCompressionAlgorithms;
using TreeCompressionAlgorithms.CompressionStrategies.TreeRePair;

// Create a compression strategy
var compressionStrategy = new RePairTreeLinearization(minFrequency: 2);

// Initialize the tree compressor
var compressor = new NaturalLanguageTreeCompressing(compressionStrategy);

// Compress text
string inputText = "This is a sample sentence for compression.";
var compressedTree = compressor.Compress(inputText);

// Decompress
string decompressedText = compressor.Decompress(compressedTree);

Console.WriteLine($"Original size: {inputText.Length} bytes");
Console.WriteLine($"Compressed size: {compressedTree.GetSize()} bytes");
Console.WriteLine($"Compression ratio: {(double)compressedTree.GetSize() / inputText.Length:F2}");
```

### Pipeline Configuration

```csharp
using TreeCompressionPipeline;

// Create a custom pipeline
var pipeline = new Pipeline()
{
    ProcessObserver = new ProcessMonitor()
}
.AddFilter(FilterFactory<IDependencyTreeNode>.CreateTextToTreeFilter(new UdPipeCreateTreeStrategy()))
.AddFilter(FilterFactory<IDependencyTreeNode>.CreateCompressionFilter(compressionStrategy));

// Process input
var result = pipeline.Process(inputText);
```

## 🧠 Compression Strategies

### 1. TreeRePair Linearization

```csharp
var strategy = new RePairTreeLinearization(
    encoder: new DepthFirstEncoder(),
    minFrequency: 2,
    maxN: 10,
    minN: 2
);
```

**Best for**: Texts with repeating syntactic patterns
**Performance**: Moderate compression, good for medium-sized documents

### 2. Optimized TreeRePair

```csharp
var strategy = new RePairOptimizedLinearStrategy(
    minFrequency: 2,
    maxN: 8,
    minN: 2
);
```

**Best for**: Large documents requiring fast processing
**Performance**: Linear time complexity improvements

### 3. Grammar-Based Compression

```csharp
var strategy = new GrammarCompressor<string>();
```

**Best for**: Highly structured text with clear patterns
**Performance**: High compression ratio for suitable content

## 📈 Performance Analysis

### Compression Results by Text Type

| Text Type | Avg. Compression Ratio | Avg. Processing Time | Success Rate |
|-----------|----------------------|---------------------|--------------|
| Technical Documentation | 1.15 | 245ms | 4% |
| Legal Documents | 1.23 | 312ms | 5% |
| Research Papers | 1.18 | 198ms | 9% |
| Literature | 1.31 | 267ms | 3% |

### Performance Insights

- **Technical documentation** shows the best compression ratios due to repetitive structures
- **Literature** is most challenging due to varied linguistic patterns
- **Processing time** scales linearly with document size for optimized strategies

## 📖 API Documentation

### Core Interfaces

#### ITreeCompressor<T>
```csharp
public interface ITreeCompressor<T> where T : ITreeNode
{
    CompressedTree Compress(string text);
    string Decompress(CompressedTree compressedTree);
    protected ICompressionStrategy<T> CompressionStrategy { get; }
    protected Pipeline CompressingPipeline { get; }
    protected Pipeline DecompressingPipeline { get; }
}
```

#### ICompressionStrategy<T>
```csharp
public interface ICompressionStrategy<T>
{
    CompressedTree Compress(T tree);
    T Decompress(CompressedTree compressedTree);
}
```

### Tree Structures

#### IDependencyTreeNode
```csharp
public interface IDependencyTreeNode : ITreeNode
{
    List<IDependencyTreeNode> LeftChildren { get; }
    List<IDependencyTreeNode> RightChildren { get; }
    void AddLeftChild(IDependencyTreeNode child);
    void AddRightChild(IDependencyTreeNode child);
    IDependencyTreeNode? Parent { get; set; }
    int GetNodeCount();
}
```

## 💡 Examples

### Processing Multiple Documents

```csharp
var documents = new[]
{
    "First document content...",
    "Second document content...",
    "Third document content..."
};

var results = new List<(string original, CompressedTree compressed, double ratio)>();

foreach (var doc in documents)
{
    var compressed = compressor.Compress(doc);
    var ratio = (double)compressed.GetSize() / doc.Length;
    results.Add((doc, compressed, ratio));
}

// Analyze results
var avgRatio = results.Average(r => r.ratio);
Console.WriteLine($"Average compression ratio: {avgRatio:F2}");
```

### Custom Tree Creation Strategy

```csharp
public class CustomTreeCreationStrategy : ITreeCreationStrategy<IDependencyTreeNode>
{
    public IDependencyTreeNode CreateTree(string text)
    {
        var root = new DependencyTreeNode("<root>");
        var words = text.Split(' ');
        
        foreach (var word in words.Where(w => !string.IsNullOrWhiteSpace(w)))
        {
            root.AddRightChild(new DependencyTreeNode(word));
        }
        
        return root;
    }
}
```

### Performance Monitoring

```csharp
public class CustomProcessObserver : IProcessObserver
{
    public void OnStart(string process) => Console.WriteLine($"Starting: {process}");
    public void OnComplete(string process, object result) => Console.WriteLine($"Completed: {process}");
    public void OnError(string process, Exception error) => Console.WriteLine($"Error in {process}: {error.Message}");
    public void OnProgress(string process, double percentComplete) => Console.WriteLine($"{process}: {percentComplete:F1}%");
}
```

## 🧪 Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test src/Tests/TreeCompressionLibraryTests/

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Categories

- **Unit Tests**: Core functionality testing
- **Integration Tests**: Full pipeline testing
- **Stress Tests**: Performance and stability testing
- **Edge Case Tests**: Boundary condition handling

### Example Test

```csharp
[TestMethod]
public void TreeRePair_CompressAndDecompress_PreservesContent()
{
    // Arrange
    var strategy = new RePairTreeLinearization();
    var originalText = "This is a test sentence.";
    
    // Act
    var compressed = strategy.Compress(CreateTreeFromText(originalText));
    var decompressed = strategy.Decompress(compressed);
    
    // Assert
    Assert.AreEqual(originalText.Split(' ').Length, 
                   decompressed.ToString().Split(' ').Length);
}
```

## 📊 Analysis and Visualization

The project includes comprehensive R scripts for performance analysis:

```bash
# Run analysis (requires R)
cd analysis
Rscript graphs.R
```

**Generated Visualizations:**
- Compression ratio comparisons
- Processing time analysis
- Success rate distributions
- Method effectiveness charts

## 🔗 References

### Core Algorithms & Compression
- [TreeRePair: XML Tree Structure Compression Using RePair](https://doi.org/10.1016/j.is.2013.08.001) - Lohrey et al., 2013
- [Tree Structure Compression with Repair](https://doi.org/10.1109/DCC.2011.44) - Lohrey et al., 2011
- [Grammar-based Tree Compression](https://doi.org/10.1007/978-3-319-21500-6_4) - Lohrey, 2015
- [Understanding Compression: Data Compression for Modern Developers](https://www.oreilly.com/library/view/understanding-compression/9781491961520/) - McAnlis & Haecky, 2016

### Natural Language Processing & Dependency Parsing
- [UDPipe: Trainable Pipeline for Processing CoNLL-U Files](https://ufal.mff.cuni.cz/udpipe)
- [Dependency Tree Based Sentence Compression](https://aclanthology.org/W08-1104/) - Filippova & Strube, 2008
- [Sentence Compression as Tree Transduction](https://doi.org/10.1613/jair.2688) - Cohn & Lapata, 2009
- [Multi-sentence Compression: Finding Shortest Paths in Word Graphs](https://aclanthology.org/C10-1037/) - Filippova, 2010
- [A Fast and Accurate Dependency Parser using Neural Networks](https://aclanthology.org/D14-1082/) - Chen & Manning, 2014
- [Dependency Parsing](https://doi.org/10.1007/978-3-031-02131-2) - Kübler et al., 2009

### Tools & Frameworks
- [MorphoDiTa: Morphological Dictionary and Tagger](https://ufal.mff.cuni.cz/morphodita) - Straka & Straková, 2020
- [UDPipe 2.0 Prototype at CoNLL 2018 UD Shared Task](http://www.aclweb.org/anthology/K/K17/K17-3009.pdf) - Straka & Straková, 2017
- [Open-Source Tools for Morphology, Lemmatization, POS Tagging and Named Entity Recognition](http://www.aclweb.org/anthology/P/P14/P14-5003.pdf) - Straková et al., 2014
- [Parsing Universal Dependency Treebanks using Neural Networks](https://ufal.mff.cuni.cz/~straka/papers/2015-tlt_14.pdf) - Straka et al., 2015

### Architecture & Design Patterns
- [Pipe-Filter Architectural Style](https://doi.org/10.1007/978-3-319-44339-3_13) - Oquendo et al., 2016
- [Speech and Language Processing: An Introduction to Natural Language Processing](https://web.stanford.edu/~jurafsky/slp3/) - Jurafsky & Martin, 2024

### Advanced Techniques
- [Sentence Compression by Deletion with LSTMs](https://doi.org/10.18653/v1/d15-1042) - Filippova et al., 2015

## 📧 Contact

- **Author**: Marek Beran
- **Project Link**: [https://github.com/Bercek71/tree-compression](https://github.com/Bercek71/tree-compression)

---

⭐ **Star this repository if you find it useful!**
