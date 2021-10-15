``` ini

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19043.1288 (21H1/May2021Update)
Intel Core i7-9700 CPU 3.00GHz, 1 CPU, 8 logical and 8 physical cores
  [Host]     : .NET Framework 4.8 (4.8.4300.0), X64 RyuJIT
  Job-KWVDKZ : .NET Framework 4.8 (4.8.4300.0), X64 RyuJIT

Runtime=.NET Framework 4.7.2  Toolchain=net472  

```
|      Method |     Mean |    Error |   StdDev |      Gen 0 |     Gen 1 | Gen 2 | Allocated |
|------------ |---------:|---------:|---------:|-----------:|----------:|------:|----------:|
| RenameNodes | 192.1 ms | 26.09 ms | 74.00 ms | 26000.0000 | 8000.0000 |     - |    154 MB |
