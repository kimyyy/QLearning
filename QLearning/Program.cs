using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLearning
{
	class Program
	{
		static void Main(string[] args)
		{
			var agent = new Agent();
			agent.QLearning();
			var result = new ResultForm();
			result.ShowDialog();

		}
	}
}
