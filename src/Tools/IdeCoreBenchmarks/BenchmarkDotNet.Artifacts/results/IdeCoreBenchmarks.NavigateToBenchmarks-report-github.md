``` ini

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19043.1237 (21H1/May2021Update)
Intel Core i7-9700 CPU 3.00GHz, 1 CPU, 8 logical and 8 physical cores
  [Host]     : .NET Framework 4.8 (4.8.4300.0), X64 RyuJIT
  Job-KWVDKZ : .NET Framework 4.8 (4.8.4300.0), X64 RyuJIT

Runtime=.NET Framework 4.7.2  Toolchain=net472  

```
|        Method |     Mean |   Error |   StdDev |   Median |      Gen 0 |     Gen 1 | Gen 2 | Allocated |
|-------------- |---------:|--------:|---------:|---------:|-----------:|----------:|------:|----------:|
| RunNavigateTo | 421.7 ms | 9.66 ms | 28.17 ms | 429.4 ms | 19000.0000 | 3000.0000 |     - |    115 MB |
