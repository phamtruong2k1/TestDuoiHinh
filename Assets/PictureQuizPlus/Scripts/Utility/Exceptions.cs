using System;

public class ReadingFileException : Exception
{
    public override string Message { get; }
    public ReadingFileException()
    {
        Message = "Reading File Error";
    }

    public ReadingFileException(string message)
        : base(message)
    {
        Message = message;
    }

    public ReadingFileException(string message, Exception inner)
        : base(message, inner)
    {
    }

}

public class FileNotExistEsception : Exception
{
    public override string Message { get; }
    public FileNotExistEsception()
    {
        Message = "File Not Exist";
    }

    public FileNotExistEsception(string message)
        : base(message)
    {
        Message = message;
    }

    public FileNotExistEsception(string message, Exception inner)
        : base(message, inner)
    {
    }

}