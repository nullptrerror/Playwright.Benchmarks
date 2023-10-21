using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace ConsoleBenchmark.Program2;

internal sealed class Program2
{
    internal const int StateSize = 10;
    internal const int ActionSize = 2;
    internal static Random random = new Random();
    internal const double Epsilon = 0.1;  // exploration rate

    internal static void Main2()
    {
        var agent = new DQNAgent();
        for (int episode = 0; episode < 1000; episode++)
        {
            int state = 0;  // start position
            while (state != 9)  // while not at the goal
            {
                var action = agent.Act(state);
                var (nextState, reward) = Step(state, action);
                agent.Learn(state, action, reward, nextState);
                state = nextState;
            }
        }

        // Test the policy
        int testState = 0;
        while (testState != 9)
        {
            var action = agent.ChooseBestAction(testState);
            testState = Step(testState, action).Item1;
            Console.WriteLine($"Position: {testState}");
        }
    }

    internal static(int, double) Step(int state, int action)
    {
        int nextState;
        if (action == 0)  // move left
            nextState = Math.Max(state - 1, 0);
        else  // move right
            nextState = Math.Min(state + 1, 9);

        double reward = nextState == 9 ? 10 : -1;
        return (nextState, reward);
    }
}

internal class DQNAgent
{
    internal Matrix<float> Q;  // Q-table
    internal const double LearningRate = 0.1;
    internal const double DiscountFactor = 0.95;

    public DQNAgent()
    {
        Q = DenseMatrix.OfArray(new float[Program2.StateSize, Program2.ActionSize]);
    }

    public int Act(int state)
    {
        // Epsilon-greedy policy
        if (Program2.random.NextDouble() < Program2.Epsilon)
            return Program2.random.Next(Program2.ActionSize);
        return ChooseBestAction(state);
    }

    public int ChooseBestAction(int state)
    {
        return Q.Row(state).MaximumIndex();
    }

    public void Learn(int state, int action, double reward, int nextState)
    {
        var target = (float)(reward + DiscountFactor * Q.Row(nextState).Maximum());
        Q[state, action] = (1 - (float)LearningRate) * Q[state, action] + (float)LearningRate * target;
    }
}