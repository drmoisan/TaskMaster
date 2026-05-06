using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;

namespace TaskMaster.Test.AppGlobals
{
    internal static class AppToDoObjectsTestUtilities
    {
        internal static void SetReadonlyField<TTarget, TValue>(
            TTarget target,
            string fieldName,
            TValue value
        )
        {
            var field = typeof(TTarget).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            field.Should().NotBeNull($"field '{fieldName}' should exist on {typeof(TTarget).Name}");
            field!.SetValue(target, value);
        }

        internal static async Task InvokePrivateAsync(object target, string methodName)
        {
            var method = target
                .GetType()
                .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

            method
                .Should()
                .NotBeNull($"method '{methodName}' should exist on {target.GetType().Name}");

            var task = method!.Invoke(target, null) as Task;
            task.Should().NotBeNull($"method '{methodName}' should return a Task");

            await task!;
        }
    }
}
