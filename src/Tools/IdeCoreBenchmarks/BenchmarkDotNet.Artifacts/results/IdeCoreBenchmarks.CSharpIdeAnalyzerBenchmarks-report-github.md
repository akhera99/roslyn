``` ini

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19043.1288 (21H1/May2021Update)
Intel Core i7-9700 CPU 3.00GHz, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.100-rc.1.21463.6
  [Host]     : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT
  Job-XMTFST : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT
  RyuJitX64  : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT

Jit=RyuJit  Platform=X64  Runtime=.NET 6.0  

```
|      Method |        Job | Toolchain |         AnalyzerName |    Mean |   Error |  StdDev |       Gen 0 |      Gen 1 | Gen 2 | Allocated |
|------------ |----------- |---------- |--------------------- |--------:|--------:|--------:|------------:|-----------:|------:|----------:|
| RunAnalyzer | Job-XMTFST |    net6.0 | CShar(...)lyzer [33] | 15.63 s | 1.385 s | 4.084 s | 275000.0000 | 82000.0000 |     - |      2 GB |
| RunAnalyzer |  RyuJitX64 |   Default | CShar(...)lyzer [33] | 17.02 s | 1.373 s | 4.049 s | 275000.0000 | 78000.0000 |     - |      2 GB |
