using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{

    public class LongCodeComputer
    {
        public IReadOnlyList<long> Memory
        {
            get
            {
                var length = _memory.Max(kvp => kvp.Key) + 1;
                var list = new List<long>(length);
                for (int i = 0; i < length; i++)
                {
                    list.Add(_memory.GetValueOrDefault(i, 0));
                }
                return list;
            }
        }
        public int InstructionPointer { get; private set; }
        public int StepCount { get; private set; }
        public OpCode CurrentOpcode => _currentInstruction.OpCode;
        public Queue<long> Inputs { get; } = new Queue<long>();
        public Queue<long> Outputs { get; } = new Queue<long>();


        private readonly SortedDictionary<int, long> _memory;
        private Instruction _currentInstruction;
        private int _relativeBase = 0;


        public LongCodeComputer(long[] initialState)
        {
            if (initialState == null)
                throw new ArgumentException("Initial state must be provided!");

            _memory = new SortedDictionary<int, long>();

            for (int i = 0; i < initialState.Length; i++)
            {
                _memory[i] = initialState[i];
            }
        }

        public LongCodeComputer(long[] initialState, long firstInput) : this(initialState)
        {
            Inputs.Enqueue(firstInput);
        }

        public LongCodeComputer(long[] initialState, IDictionary<int, long> stateOverride) : this(initialState)
        {
            foreach (var pair in stateOverride)
            {
                _memory[pair.Key] = pair.Value;
            }
        }

        private long GetInput() => Inputs.Dequeue();

        private void SetOutput(long value) => Outputs.Enqueue(value);

        /// <summary>
        /// Adds the given input and runs the computer until an output occurs.
        /// Returns the first output or null, if the comuter halts.
        /// </summary>
        public long? RunWith(long input)
        {
            Inputs.Enqueue(input);
            while (CurrentOpcode != OpCode.Halt)
            {
                ExecuteStep();
                if (CurrentOpcode == OpCode.SaveOutput)
                    return Outputs.Dequeue();
            }
            return null;
        }

        public void Run(int maxSteps = int.MaxValue)
        {
            while (maxSteps-- > 0 && CurrentOpcode != OpCode.Halt)
                ExecuteStep();
        }

        private void ExecuteStep()
        {
            StepCount++;
            _currentInstruction = GetInstruction();

            switch (_currentInstruction.OpCode)
            {
                case OpCode.Add:
                    ExecuteInstruction((a, b) => checked(a + b));
                    break;
                case OpCode.Mul:
                    ExecuteInstruction((a, b) => checked(a * b));
                    break;
                case OpCode.ReadInput:
                    ExecuteInstruction(GetInput);
                    break;
                case OpCode.SaveOutput:
                    ExecuteInstruction(a => SetOutput(a));
                    break;

                case OpCode.JmpIfTrue:
                    EvaluateJumpInstruction(x => x != 0);
                    break;
                case OpCode.JmpIfFalse:
                    EvaluateJumpInstruction(x => x == 0);
                    break;

                case OpCode.LessThan:
                    ExecuteInstruction((a, b) => a < b ? 1 : 0);
                    break;
                case OpCode.Equals:
                    ExecuteInstruction((a, b) => a == b ? 1 : 0);
                    break;

                case OpCode.SetRelativeBase:
                    _relativeBase += (int)GetNextArg();
                    break;

                case OpCode.Halt:
                    return;
                default:
                    throw new InvalidOperationException("Unknown opcode: " + _currentInstruction.OpCode);
            }
        }
        private Instruction GetInstruction()
            => new Instruction(_memory[InstructionPointer], InstructionPointer++);


        private long EvaluateInstruction(Func<long, long, long> action)
            => action(GetNextArg(), GetNextArg());

        private void EvaluateJumpInstruction(Func<long, bool> condition)
            => EvaluateInstruction((a, b) => condition(a) ? InstructionPointer = (int)b : 0);

        private void ExecuteInstruction(Func<long, long, long> action)
        {
            var result = EvaluateInstruction(action);
            var resultAddress = GetNextAddr(_currentInstruction);
            Store(resultAddress, result);
        }

        private void ExecuteInstruction(Func<long> action)
        {
            var resultAddress = GetNextAddr(_currentInstruction);
            Store(resultAddress, action());
        }

        private void ExecuteInstruction(Action<long> action)
            => action(GetNextArg());


        private long Load(int address)
        {
            if (address < 0)
                throw new InvalidOperationException("Negative addresses are forbidden!");
            return _memory.GetOrAdd(address, 0);
        }

        private void Store(int address, long value)
        {
            _memory[address] = value;
        }

        private long GetNextArg()
        {
            var paramIndex = InstructionPointer - (_currentInstruction.Location + 1);
            var paramMode = _currentInstruction.ParameterModes[paramIndex];
            var argument = Load(InstructionPointer++);

            return paramMode switch
            {
                ParameterMode.PositionMode => Load((int)argument),
                ParameterMode.ImmediateMode => argument,
                ParameterMode.RelativeMode => Load(_relativeBase + (int)argument),
                _ => throw new InvalidOperationException("Unknown parameter mode: " + paramMode),
            };
        }
        private int GetNextAddr(Instruction instruction)
        {
            var paramIndex = InstructionPointer - (instruction.Location + 1);
            var argument = (int)Load(InstructionPointer++);

            return (instruction.ParameterModes[paramIndex]) switch
            {
                ParameterMode.PositionMode => argument,
                ParameterMode.RelativeMode => _relativeBase + argument,
                _ => throw new InvalidOperationException("Invalid or unknown parameter mode: " + instruction.ParameterModes[paramIndex]),
            };
        }

        private struct Instruction
        {
            public OpCode OpCode { get; }
            public readonly ParameterMode[] ParameterModes;

            public readonly int Location { get; }

            public Instruction(long instructionCode, int location)
            {
                Location = location;

                OpCode = (OpCode)(instructionCode % 100);
                instructionCode /= 100;
                ParameterModes = new ParameterMode[4];

                for (int i = 0; i < ParameterModes.Length; i++)
                {
                    ParameterModes[i] = (ParameterMode)(instructionCode % 10);
                    instructionCode /= 10;
                }
            }

            public override string ToString() => $"{OpCode} at {Location}";
        }


        public enum ParameterMode
        {
            PositionMode = 0,
            ImmediateMode = 1,
            RelativeMode = 2
        }

        public enum OpCode
        {
            Add = 1,
            Mul = 2,
            ReadInput = 3,
            SaveOutput = 4,
            JmpIfTrue = 5,
            JmpIfFalse = 6,
            LessThan = 7,
            Equals = 8,
            SetRelativeBase = 9,
            Halt = 99
        }
    }
}