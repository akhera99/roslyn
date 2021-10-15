``` ini

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19043.1237 (21H1/May2021Update)
Intel Core i7-9700 CPU 3.00GHz, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.100-rc.1.21463.6
  [Host]     : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT
  Job-HUOHKX : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT

Runtime=.NET 6.0  Toolchain=net6.0  

```
|           Method |    Mean |   Error |  StdDev |       Gen 0 |      Gen 1 | Gen 2 | Allocated |
|----------------- |--------:|--------:|--------:|------------:|-----------:|------:|----------:|
| ClassifyDocument | 14.04 s | 0.122 s | 0.114 s | 260000.0000 | 73000.0000 |     - |      2 GB |
