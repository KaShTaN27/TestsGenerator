using System.Threading.Tasks.Dataflow;
using Core;
using Core.Model;
using JetBrains.ReSharper.TestRunner.Abstractions.Extensions;

namespace TestsGeneratorApplication;

public class PipelineService {
    private TestsGenerator _testsGenerator = new();

    private readonly TransformBlock<string, string> _blockRead;
    private readonly TransformManyBlock<string, GeneratedTestClass> _blockProcess;
    private readonly ActionBlock<GeneratedTestClass> _blockWrite;
    private string _readingPath;
    private string _savingPath;

    public PipelineService(int readOptions, int writeOptions, int processOptions, string readingPath) {
        var options = InitializeDataflowOptions(readOptions, writeOptions, processOptions);
        _readingPath = readingPath;
        _savingPath = CheckSavingPath(_readingPath);

        _blockRead = new TransformBlock<string, string>(async filePath =>
                await File.ReadAllTextAsync(filePath)
            , options[0]);

        _blockProcess = new TransformManyBlock<string, GeneratedTestClass>(sourceCode =>
                _testsGenerator.Generate(sourceCode)
            , options[1]);

        _blockWrite = new ActionBlock<GeneratedTestClass>(async testsFile =>
                await File.WriteAllTextAsync(_savingPath + testsFile.FileName,
                    testsFile.ClassCode)
            , options[2]);

        var dataflowLinkOptions = new DataflowLinkOptions { PropagateCompletion = true };

        _blockRead.LinkTo(_blockProcess, dataflowLinkOptions);
        _blockProcess.LinkTo(_blockWrite, dataflowLinkOptions);
    }

    private string CheckSavingPath(string readingPath) {
        var savingPath = readingPath + "\\Tests\\";
        if (!Directory.Exists(savingPath))
            Directory.CreateDirectory(savingPath);
        return savingPath;
    }

    private static ExecutionDataflowBlockOptions[] InitializeDataflowOptions(int read, int write, int process) {
        var options = new ExecutionDataflowBlockOptions[3];
        options[0] = new ExecutionDataflowBlockOptions() {
            MaxDegreeOfParallelism = read
        };
        options[1] = new ExecutionDataflowBlockOptions() {
            MaxDegreeOfParallelism = write
        };
        options[2] = new ExecutionDataflowBlockOptions() {
            MaxDegreeOfParallelism = process
        };
        return options;
    }

    public async Task Generate() {
        var fileNames = Directory.GetFiles(_readingPath);
        fileNames.ForEach(fileName => _blockRead.Post(fileName));
        _blockRead.Complete();
        await _blockWrite.Completion;
    }
}