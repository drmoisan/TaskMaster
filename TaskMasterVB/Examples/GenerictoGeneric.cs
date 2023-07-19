using System;

public class Class1
{
    void Run()
    {
        AnotherMethod("something", (t, u) => GenericMethod(t, u));
    }

    void GenericMethod<T, U>(T obj, U parm1)
    {
        Console.WriteLine("{0}, {1}", typeof(T).Name, typeof(U).Name);
    }

    void AnotherMethod(string parm1, Action<dynamic, dynamic> method)
    {
        method("test", 1);
        method(42, "hello world!");
        method(1.2345, "etc.");
    }
}
