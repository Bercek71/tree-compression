# :rocket: Použití

Tato sekce popisuje, jak používat konzolovou aplikaci pro **kompresi a dekompresi stromových struktur**. Aplikace podporuje různé příkazy a parametry.

---

## :gear: Základní příkaz

Pro zobrazení nápovědy použijte:

```bash
tree-compression --help
```

---

## :hammer_and_wrench: Příkazy a parametry

### :package: Komprese

Pro kompresi vstupního souboru použijte:

```bash
tree-compression compress --input <vstupni_soubor> --output <vystupni_soubor> --alg <algoritmus>
```

!!! example "Příklad použití"
    ```bash
    tree-compression compress --input data.txt --output data_compressed.txt --alg TreeRePair
    ```

=== ":page_facing_up: Parametry"
    - `--input` (**povinné**) – Cesta k vstupnímu souboru.
    - `--output` (**povinné**) – Cesta k výstupnímu souboru.
    - `--alg` (**volitelné**) – Použitý kompresní algoritmus:
        - `TreeRePair`
        - `Dictionary`

---

### :unlock: Dekomprese

Pro dekompresi souboru použijte:

```bash
tree-compression decompress --input <vstupni_soubor> --output <vystupni_soubor>
```

!!! example "Příklad použití"
    ```bash
    tree-compression decompress --input data_compressed.txt --output data_decompressed.txt
    ```

=== ":page_facing_up: Parametry"
    - `--input` (**povinné**) – Cesta k souboru s komprimovanými daty.
    - `--output` (**povinné**) – Cesta k výstupnímu souboru.

---

### :bar_chart: Analýza textu

Pro analýzu textu a generování stromové struktury použijte:

```bash
tree-compression analyze --input <vstupni_soubor> --output <vystupni_soubor>
```

!!! example "Příklad použití"
    ```bash
    tree-compression analyze --input sentences.txt --output tree_structure.json
    ```

=== ":page_facing_up: Parametry"
    - `--input` (**povinné**) – Cesta k textovému souboru.
    - `--output` (**povinné**) – Cesta k souboru, kam se uloží stromová struktura.

---

### :stopwatch: Výkonnostní testy

Pro spuštění výkonnostních testů použijte:

```bash
tree-compression benchmark --input <vstupni_soubor> --alg <algoritmus>
```

!!! example "Příklad použití"
    ```bash
    tree-compression benchmark --input large_data.txt --alg TreeRePair
    ```

=== ":page_facing_up: Parametry"
    - `--input` (**povinné**) – Cesta k souboru, který bude použit pro testování.
    - `--alg` (**volitelné**) – Algoritmus, který bude testován:
        - `TreeRePair`
        - `Dictionary`

---

### :art: Vizualizace stromové struktury

Pro vizualizaci stromové struktury použijte:

```bash
tree-compression visualize --input <vstupni_soubor>
```

!!! example "Příklad použití"
    ```bash
    tree-compression visualize --input tree_structure.json
    ```

=== ":page_facing_up: Parametry"
    - `--input` (**povinné**) – Cesta k souboru obsahujícímu stromovou strukturu.

---

## :bulb: Tipy a triky

- :loud_sound: **Zobrazte podrobné informace** pomocí `--verbose`:
  ```bash
  tree-compression compress --input data.txt --output data_compressed.txt --verbose
  ```

- :fast_forward: **Použijte více vláken pro rychlejší zpracování** pomocí `--threads`:
  ```bash
  tree-compression compress --input data.txt --output data_compressed.txt --threads 4
  ```

- :bug: **Pro ladění použijte `--debug`**:
  ```bash
  tree-compression analyze --input sentences.txt --output tree_structure.json --debug
  ```

---

### :bookmark_tabs: Shrnutí

Tato stránka poskytuje přehled všech dostupných **příkazů a parametrů**, které vám umožní efektivně pracovat s aplikací.

