# Architektura systému

## 🏛️ Modulární struktura

Systém je rozdělen do několika modulů, zajišťujících flexibilitu a efektivitu.

### 📦 Analýza textu
Modul pro syntaktickou analýzu a generování stromových struktur z textů.

- **Výstupy**: Generované stromové struktury.
- **Použité technologie**: .NET

### 🧠 Detekce vzorců
Tento modul identifikuje opakující se vzory ve stromových strukturách, které lze komprimovat.

- **Výstupy**: Seznam vzorců pro kompresi.
- **Použité technologie**: Algoritmy pro detekci vzorců.

### ⚙️ Kompresní algoritmy
Implementace kompresních metod pro optimalizaci stromových struktur.

- **Typy algoritmů**: Huffmanovo kódování, LZW.
- **Výstupy**: Komprimované datové struktury.

> **Tip**: Každý modul je navržen pro snadnou rozšiřitelnost a testování nových metod.

## 💡 Výhody modulární struktury

- **Flexibilita**: Snadno přidáváte nové moduly.
- **Škálovatelnost**: Podporuje práci s různými velikostmi dat.
- **Paralelizace**: Každý modul lze paralelizovat pro zrychlení výpočtů.


```plantuml
@startuml Tree Compression System

' Main interfaces
interface IFilter {
    +Process(data: object): object
    +Chain(nextFilter: IFilter): IFilter
}

interface ITreeNode {
    +Value: object
    +Children: IList<ITreeNode>
    +AddChild(child: ITreeNode): void
    +Accept(visitor: ITreeVisitor): void
}

interface ITreeVisitor {
    +Visit(node: ITreeNode): void
}

interface ICompressionStrategy {
    +Compress(tree: ITreeNode): CompressedTree
    +Decompress(compressedTree: CompressedTree): ITreeNode
}

' Abstract factory for filters
abstract class FilterFactory {
    +{static} CreateSyntacticAnalysisFilter(): IFilter
    +{static} CreateCompressionFilter(strategy: ICompressionStrategy): IFilter
    +{static} CreateDecompressionFilter(strategy: ICompressionStrategy): IFilter
}

' Concrete filters
class SyntacticAnalysisFilter implements IFilter {
    -nextFilter: IFilter
    +Process(text: object): object
    +Chain(nextFilter: IFilter): IFilter
}


class CompressionFilter implements IFilter {
    -nextFilter: IFilter
    -strategy: ICompressionStrategy
    +CompressionFilter(strategy: ICompressionStrategy)
    +Process(tree: object): object
    +Chain(nextFilter: IFilter): IFilter
}

class DecompressionFilter implements IFilter {
    -nextFilter: IFilter
    -strategy: ICompressionStrategy
    +DecompressionFilter(strategy: ICompressionStrategy)
    +Process(compressedTree: object): object
    +Chain(nextFilter: IFilter): IFilter
}

' Tree structure (Composite pattern)
class TreeNode implements ITreeNode {
    -value: object
    -children: List<ITreeNode>
    +Value: object
    +Children: IList<ITreeNode>
    +AddChild(child: ITreeNode): void
    +Accept(visitor: ITreeVisitor): void
}

class CompressedTree {
    +Patterns: Dictionary<int, ITreeNode>
    +Structure: byte[]
    +Metadata: Dictionary<string, string>
}

' Strategy pattern for compression
abstract class CompressionStrategy implements ICompressionStrategy {
    #FindPatterns(tree: ITreeNode): Dictionary<string, int>
    +Compress(tree: ITreeNode): CompressedTree
    +Decompress(compressedTree: CompressedTree): ITreeNode
}

class PatternBasedCompression extends CompressionStrategy {
    -minPatternSize: int
    -maxPatterns: int
    +PatternBasedCompression(minPatternSize: int, maxPatterns: int)
    +Compress(tree: ITreeNode): CompressedTree
    +Decompress(compressedTree: CompressedTree): ITreeNode
}

class StatisticalCompression extends CompressionStrategy {
    -threshold: double
    +StatisticalCompression(threshold: double)
    +Compress(tree: ITreeNode): CompressedTree
    +Decompress(compressedTree: CompressedTree): ITreeNode
}

' Pipeline (Pipes and Filters implementation)
class Pipeline {
    -firstFilter: IFilter
    -lastFilter: IFilter
    +AddFilter(filter: IFilter): Pipeline
    +Process(input: object): object
}

class TextProcessingFacade {
    -pipeline: Pipeline
    +TextProcessingFacade()
    +CompressText(text: string): CompressedTree
    +DecompressTree(compressedTree: CompressedTree): string
    +ValidateDecompression(originalText: string, decompressedText: string): bool
}

' Observer Pattern for monitoring
interface IProcessObserver {
    +OnStart(process: string): void
    +OnProgress(process: string, percentComplete: double): void
    +OnComplete(process: string, result: object): void
    +OnError(process: string, error: Exception): void
}

class ProcessMonitor implements IProcessObserver {
    +OnStart(process: string): void
    +OnProgress(process: string, percentComplete: double): void
    +OnComplete(process: string, result: object): void
    +OnError(process: string, error: Exception): void
}

' Relationships
Pipeline --> IFilter
IFilter --> IFilter: chain
FilterFactory ..> IFilter: creates
CompressionFilter --> ICompressionStrategy: uses
DecompressionFilter --> ICompressionStrategy: uses
TextProcessingFacade --> Pipeline: uses
TreeNode --> ITreeNode: contains
CompressionStrategy --> CompressedTree: produces
CompressionStrategy --> ITreeNode: consumes/produces
TextProcessingFacade ..> IProcessObserver: notifies

@enduml
```