using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLearning
{
	class Agent
	{
		#region フィールド

		/// <summary>
		/// Agentが現在いるx座標
		/// </summary>
		private int posx;

		/// <summary>
		/// Agentが現在いるy座標
		/// </summary>
		private int posy;

		/// <summary>
		/// 経過した時間
		/// </summary>
		private int time;

		/// <summary>
		/// 学習係数
		/// </summary>
		private readonly double alpha = 0.1;

		/// <summary>
		/// 割引率
		/// </summary>
		private readonly double drate = 0.9;

		/// <summary>
		/// e-Greedy法に用いるepsilon
		/// </summary>
		private readonly double epsilon = 0.1;

		/// <summary>
		/// Q値。3つ目のインデックスは方角を表す。North = 1,East = 2, West = 3, South = 4。
		/// </summary>
		public static double[,,] Q = new double[7, 7, 5];

		/// <summary>
		/// ステージの様子を表します。0なら何もなし、G1なら2,G2なら1,壁にはpenaltyが入る
		/// </summary>
		public static double[,] Stage = new double[8, 8];

		public static int[,] visit = new int[7, 7];

		/// <summary>
		/// 壁に当たったときに付与するペナルティです。
		/// </summary>
		private readonly double penalty = -0.7;

		/// <summary>
		/// ゴールを訪れた回数
		/// </summary>
		private ulong counter = 0;

		private Random rand;

		#endregion

		#region メソッド

		/// <summary>
		/// ステージ1を生成します。
		/// </summary>
		private void SetStage()
		{
			for (int i = 0; i < 7; i++)
			{
				Stage[0, i] = Stage[7, i] = Stage[i, 0] = Stage[i, 7] 
					= penalty;
			}

			Stage[5, 4] = 1;
		}

		/// <summary>
		/// ステージ2を生成します。
		/// </summary>
		private void SetStage2()
		{
			for (int i = 0; i < 7; i++)
			{
				Stage[0, i] = Stage[7, i] = Stage[i, 0] = Stage[i, 7] = penalty;
			}

			Stage[5, 4] = 2;
			Stage[2, 5] = 1;

			Stage[2, 2] = Stage[3, 2] = Stage[4, 2] = Stage[5, 2] = penalty;

			Stage[3, 4] = Stage[3, 5] = Stage[5, 5] = Stage[6, 5] = penalty;
		}

		private void SetStage3()
		{
			for (int i = 0; i < 7; i++)
			{
				Stage[0, i] = Stage[7, i] = Stage[i, 0] =
					Stage[i, 7] = penalty;
			}

			Stage[6, 6] = 2;

			Stage[2, 2] = Stage[3, 2] = Stage[4, 2] = Stage[5, 2] = penalty;

			Stage[3, 4] = Stage[3, 5] = Stage[5, 5] = Stage[6, 5] = penalty;
		}

		/// <summary>
		/// 次の状態を決定します。
		/// </summary>
		/// <param name="nextx"></param>
		/// <param name="nexty"></param>
		/// <param name="direction"></param>
		/// <returns></returns>
		private double GetNextState(ref int nextx, ref int nexty, int direction)
		{
			if (direction == 1)
			{
				nextx = posx;
				if (Stage[posx, posy - 1] >= 0)
				{
					nexty = posy - 1;
				}
				else
				{
					nexty = posy;
				}
				return Stage[posx, posy - 1];
			}
			else if (direction == 2)
			{
				nexty = posy;
				if (Stage[posx + 1, posy] >= 0)
				{
					nextx = posx + 1;
				}
				else
				{
					nextx = posx;
				}
				return Stage[posx + 1, posy];
			}
			else if (direction == 3)
			{
				nexty = posy;
				if (Stage[posx - 1, posy] >= 0)
				{
					nextx = posx - 1;
				}
				else
				{
					nextx = posx;
				}
				return Stage[posx - 1, posy];
			}
			else
			{
				// Southの場合
				nextx = posx;
				if (Stage[posx, posy + 1] >= 0)
				{
					nexty = posy + 1;
				}
				else
				{
					nexty = posy;
				}
				return Stage[posx, posy + 1];
			}
		}

		/// <summary>
		/// スタート地点に戻ります。
		/// </summary>
		private void GoStart()
		{
			posx = 1;
			posy = 1;
		}

		/// <summary>
		/// ランダムな場所に行きます。
		/// </summary>
		private void GoRandom()
		{
			Random r = new Random();
			while (Stage[posx, posy] != 0)
			{
				posx = r.Next(6) + 1;
				posy = r.Next(6) + 1;
			}
		}

		/// <summary>
		/// Q値が最大である方角を調べます。
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		private int SearchMaxDirection(int x, int y)
		{
			// 最大のQを与える方角を探す。
			double q = Q[x, y, 1];
			int max = 1;
			for (int i = 2; i < 5; i++)
			{
				if (Q[x, y, i] > q)
				{
					max = i;
				}
			}
			return max;
		}

		/// <summary>
		/// e-Greedy法により次の行動を決定します。
		/// </summary>
		/// <returns></returns>
		private int Greedy()
		{
			int maxNum = 1;
			double max = Q[posx, posy, 1];
			for (int i = 2; i < 5; i++)
			{
				double q = Q[posx, posy, i];
				if (q > max)
				{
					maxNum = 1;
					max = q;
				}
				else if (q == max)
				{
					maxNum++;
				}
			}
			double[] prob = new double[5];
			for (int i = 1; i < 5; i++)
			{
				if (Q[posx, posy, i] == max)
				{
					prob[i] = (1 - epsilon) / maxNum;
				}
				else
				{
					prob[i] = epsilon / (4 - maxNum);
				}
			}
			double d = rand.NextDouble();
			if (d < prob[1])
			{
				return 1;
			}
			else if (prob[1] <= d && d < prob[1] + prob[2])
			{
				return 2;
			}
			else if (prob[1] + prob[2] <= d &&
				d < prob[1] + prob[2] + prob[3])
			{
				return 3;
			}
			else
			{
				return 4;
			}
		}

		/// <summary>
		/// Softmax法により次の行動を決定します。
		/// </summary>
		/// <returns></returns>
		private int SoftMax()
		{
			double sum = 0;
			for (int i = 1; i < 5; i++)
			{
				sum += Math.Exp(Q[posx, posy, i]);
			}
			double threshold1_2 = Math.Exp(Q[posx, posy, 1]) / sum;
			double threshold2_3 = threshold1_2 +
				Math.Exp(Q[posx, posy, 2]) / sum;
			double threshold3_4 = threshold2_3 +
				Math.Exp(Q[posx, posy, 3]) / sum;
			double d = rand.NextDouble();
			if (d < threshold1_2)
			{
				return 1;
			}
			else if (threshold1_2 <= d && d < threshold2_3)
			{
				return 2;
			}
			else if (threshold2_3 <= d && d < threshold3_4)
			{
				return 3;
			}
			else
			{
				return 4;
			}
		}

		#endregion

		/// <summary>
		/// 学習の単位。今のところ1回の行動。
		/// </summary>
		private void Learning()
		{
			if(posx == 6 && posy == 1)
			{
				counter++;
			}
			// 次の行動を決定
			int nextDirection = Greedy();

			int nextx = 0;
			int nexty = 0;

			// 次の状態と強化信号を取得
			double signal = GetNextState(ref nextx, ref nexty, nextDirection);

			// Q値を更新
			Q[posx, posy, nextDirection] =
				(1 - alpha) * Q[posx, posy, nextDirection] +
				alpha * (signal + drate * 
				Q[nextx, nexty, SearchMaxDirection(nextx, nexty)]);

			//状態を更新
			posx = nextx;
			posy = nexty;

			// 探索回数を記録
			visit[posx, posy]++;

			// ゴールにいるなら飛ぶ
			if (Stage[posx, posy] > 0)
			{
				counter++;
				GoRandom();
			}
			time += 1;
		}

		/// <summary>
		/// Q学習法をします。
		/// </summary>
		public void QLearning()
		{
			SetStage();

			rand = new Random();

			time = 0;

			GoStart();

			while (time < 1000000)
			{
				Learning();
			}
		}
	}
}
