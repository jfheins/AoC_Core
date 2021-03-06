using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Core.Test
{
    [TestClass]
    public class LongCodeComputerTests
    {
        private void CollectionAssertAreEqual<T>(IEnumerable<T> a, IEnumerable<T> b)
        {
            var index = 0;
            foreach (var (First, Second) in a.Zip(b))
                Assert.AreEqual<T>(First, Second, $"Index {index}: Expected {First} but saw {Second}");
        }

        [TestMethod]
        public void CanCreate()
        {
            var state = new long[] { 17 };
            var computer = new LongCodeComputer(state);

            Assert.IsTrue(computer.Memory.Count == 1);
            Assert.IsTrue(computer.Memory[0] == 17);
        }

        [TestMethod]
        public void CanOverride()
        {
            var state = new long[] { 17, 33 };
            var computer = new LongCodeComputer(state, new Dictionary<int, long> { { 1, 6 } });

            Assert.IsTrue(computer.Memory.Count == 2);
            Assert.IsTrue(computer.Memory[0] == 17);
            Assert.IsTrue(computer.Memory[1] == 6);
        }

        [TestMethod]
        public void Program_1()
        {
            var computer = new LongCodeComputer(new long[] { 1, 0, 0, 0, 99 });
            computer.Run();
            var expected = new long[] { 2, 0, 0, 0, 99 };
            CollectionAssertAreEqual(expected, computer.Memory);
            Assert.AreEqual(2, computer.StepCount);
        }

        [TestMethod]
        public void Program_2()
        {
            var computer = new LongCodeComputer(new long[] { 2, 3, 0, 3, 99 });
            computer.Run();
            var expected = new long[] { 2, 3, 0, 6, 99 };
            CollectionAssertAreEqual(expected, computer.Memory);
        }

        [TestMethod]
        public void Program_3()
        {
            var computer = new LongCodeComputer(new long[] { 2, 4, 4, 5, 99, 0 });
            computer.Run();
            var expected = new long[] { 2, 4, 4, 5, 99, 9801 };
            CollectionAssertAreEqual(expected, computer.Memory);
        }

        [TestMethod]
        public void Program_4()
        {
            var computer = new LongCodeComputer(new long[] { 1, 1, 1, 4, 99, 5, 6, 0, 99 });
            computer.Run();
            var expected = new long[] { 30, 1, 1, 4, 2, 5, 6, 0, 99 };
            CollectionAssertAreEqual(expected, computer.Memory);
        }

        [TestMethod]
        public void Program_D5_negative_instr()
        {
            var computer = new LongCodeComputer(new long[] { 1101, 100, -1, 4, 0 });
            computer.Run(5);
            var expected = new long[] { 1101, 100, -1, 4, 99 };
            CollectionAssertAreEqual(expected, computer.Memory);
        }

        [TestMethod]
        public void LimitedExecution()
        {
            var computer = new LongCodeComputer(new long[] { 1, 1, 1, 4, 99, 5, 6, 0, 99 });
            computer.Run(1);
            var expected = new long[] { 1, 1, 1, 4, 2, 5, 6, 0, 99 };
            CollectionAssertAreEqual(expected, computer.Memory);
        }

        [TestMethod]
        public void InputOutputWorks()
        {
            var computer = new LongCodeComputer(new long[] { 3, 0, 4, 0, 99 });
            computer.Inputs.Enqueue(37);
            computer.Run();
            Assert.AreEqual(37, computer.Outputs.First());
        }

        [TestMethod]
        public void InputOutputWorks_WithCall()
        {
            var computer = new LongCodeComputer(new long[] { 3, 0, 4, 0, 99 });
            var result = computer.RunWith(37);
            Assert.AreEqual(37, result);
        }

        [DataTestMethod]
        [DataRow(6, 999)]
        [DataRow(7, 999)]
        [DataRow(8, 1000)]
        [DataRow(9, 1001)]
        [DataRow(10, 1001)]
        [DataRow(11, 1001)]
        public void InputOutputWorks2(long input, long expected)
        {
            var computer = new LongCodeComputer(new long[] { 3, 21, 1008, 21, 8, 20, 1005, 20, 22,
                107, 8, 21, 20, 1006,20, 31, 1106, 0, 36, 98, 0, 0, 1002, 21, 125, 20, 4, 20,
                1105, 1, 46, 104, 999, 1105, 1, 46, 1101, 1000, 1, 20, 4, 20, 1105, 1, 46, 98, 99 });
            computer.Inputs.Enqueue(input);
            computer.Run();
            Assert.AreEqual(expected, computer.Outputs.First());
        }

        [TestMethod]
        public void ImmediateParamWorks()
        {
            var computer = new LongCodeComputer(new long[] { 1002, 4, 3, 4, 33 });
            computer.Run(2);
            var expected = new long[] { 1002, 4, 3, 4, 99 };
            CollectionAssertAreEqual(expected, computer.Memory);
        }

        [DataTestMethod]
        [DataRow(6, 0)]
        [DataRow(7, 0)]
        [DataRow(8, 1)]
        [DataRow(9, 0)]
        public void CompareJump_1_equals_8(long input, long expected)
        {
            var computer = new LongCodeComputer(new long[] { 3, 9, 8, 9, 10, 9, 4, 9, 99, -1, 8 });
            computer.Inputs.Enqueue(input);
            computer.Run(10);
            Assert.AreEqual(expected, computer.Outputs.First());
        }

        [DataTestMethod]
        [DataRow(6, 1)]
        [DataRow(7, 1)]
        [DataRow(8, 0)]
        [DataRow(9, 0)]
        public void CompareJump_2_lessthan_8(long input, long expected)
        {
            var computer = new LongCodeComputer(new long[] { 3, 9, 7, 9, 10, 9, 4, 9, 99, -1, 8 });
            computer.Inputs.Enqueue(input);
            computer.Run(10);
            Assert.AreEqual(expected, computer.Outputs.First());
        }

        [DataTestMethod]
        [DataRow(6, 0)]
        [DataRow(7, 0)]
        [DataRow(8, 1)]
        [DataRow(9, 0)]
        public void CompareJump_3_equal_8(long input, long expected)
        {
            var computer = new LongCodeComputer(new long[] { 3, 3, 1108, -1, 8, 3, 4, 3, 99 });
            computer.Inputs.Enqueue(input);
            computer.Run(10);
            Assert.AreEqual(expected, computer.Outputs.First());
        }

        [DataTestMethod]
        [DataRow(6, 1)]
        [DataRow(7, 1)]
        [DataRow(8, 0)]
        [DataRow(9, 0)]
        public void CompareJump_4_lessthan_8(long input, long expected)
        {
            var computer = new LongCodeComputer(new long[] { 3, 3, 1107, -1, 8, 3, 4, 3, 99 });
            computer.Inputs.Enqueue(input);
            computer.Run(10);
            Assert.AreEqual(expected, computer.Outputs.First());
        }

        // Can read bexond program bounds

        [TestMethod]
        public void TestRelativeBase()
        {
            var program = new long[] { 109, 1, 204, -1, 1001, 100, 1, 100, 1008, 100, 16, 101, 1006, 101, 0, 99 };
            var computer = new LongCodeComputer(program);
            computer.Run();
            CollectionAssert.AreEqual(program, computer.Outputs);
        }

        [TestMethod]
        public void TestLargeNumbers()
        {
            var program = new long[] { 104, 1125899906842624, 99 };
            var computer = new LongCodeComputer(program);
            computer.Run();
            Assert.AreEqual(program[1], computer.Outputs.First());
        }
    }
}
