//using BenchmarkDotNet.Running;

#if true
Testing.TestOutMultipleWithEntityHeader();
#else
var summary = BenchmarkRunner.Run<StructCopyBenchmarks>();
#endif

