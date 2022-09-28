using DisCatSharp.EventArgs;

using Name.Bayfaderix.Darxxemiyur.Common;

namespace Tests
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void TestMethod1()
		{
		}
		[TestMethod("TaskTest")]
		public async Task TestMethod2()
		{
			TaskEventProxy<int> proxy = new();

			await Task.WhenAll(Gen(proxy), Read(proxy));
		}
		private async Task Gen(TaskEventProxy<int> proxy)
		{
			await Task.WhenAll(Enumerable.Range(1, 500).Select(x => proxy.Handle(x)));
		}
		private async Task Read(TaskEventProxy<int> proxy)
		{
			while (await proxy.HasAny())
			{
				Console.WriteLine((await proxy.GetData()));
			}
		}
	}
}