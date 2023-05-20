using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gurobi;

namespace Aula2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void ProblemaTransporte_Click(object sender, EventArgs e)
        {
            Random Aleatorio = new Random(1);
            int NumOrigens = 50;
            int NumDestinos = 50;
            //int OfertaTotal = 75;
            int[,] CustosVariaveis = new int[NumOrigens, NumDestinos];
            int[,] CustosFixos = new int[NumOrigens, NumDestinos];
            int[] Ofertas = new int[NumOrigens];
            int[] Demandas = new int[NumDestinos];
            int[,] MatrizM = new int[NumOrigens, NumDestinos];
            GRBEnv Ambiente = new GRBEnv();
            GRBModel Modelo = new GRBModel(Ambiente);
            GRBVar[,] X = new GRBVar[NumOrigens, NumDestinos];
            GRBVar[,] W = new GRBVar[NumOrigens, NumDestinos];

            //Gerar custos aleatórios
            for (int i = 0; i < NumOrigens; i++)
            {
                for (int j = 0; j < NumDestinos; j++)
                {
                    CustosVariaveis[i, j] = Aleatorio.Next(3, 15);
                    CustosFixos[i, j] = Aleatorio.Next(300, 501);
                }
            }

            //Definir a oferta de cada origem
            int Soma = 0;
            for (int i = 0; i < NumOrigens; i++)
            {
                Ofertas[i] = Aleatorio.Next(80, 121);
                Soma += Ofertas[i];
            }
            //int Diferenca = OfertaTotal - Soma;
            //if(Diferenca>0)
            //{
            // for (int d = 0; d < Diferenca; d++)
            // {
            // int OrigemEscolhida = Aleatorio.Next(0, NumOrigens);
            // Ofertas[OrigemEscolhida] += 1;
            // }
            //}
            //else if(Diferenca<0)
            //{
            // for(int d=0;d<Diferenca;d++)
            // {
            // int OrigemEscolhida = Aleatorio.Next(0, NumOrigens);
            // while(Ofertas[OrigemEscolhida]<=1)
            // {
            // OrigemEscolhida = Aleatorio.Next(0, NumOrigens);
            // }
            // Ofertas[OrigemEscolhida] -= 1;
            // }
            //}

            //Definir a demanda de cada destino
            for (int j = 0; j < NumDestinos; j++)
            {
                Demandas[j] = Aleatorio.Next(80, 101);
            }

            //Criar Matriz M
            for (int i = 0; i < NumOrigens; i++)
            {
                for (int j = 0; j < NumDestinos; j++)
                {
                    if (Ofertas[i] <= Demandas[j])
                    {
                        MatrizM[i, j] = Ofertas[i];
                    }
                    else
                    {
                        MatrizM[i, j] = Demandas[j];
                    }
                }
            }

            //Definir as variáveis de decisão do modelo e a função objetivo
            GRBLinExpr FuncaoObjetivo = new GRBLinExpr();
            for (int i = 0; i < NumOrigens; i++)
            {
                for (int j = 0; j < NumDestinos; j++)
                {
                    X[i, j] = Modelo.AddVar(0, double.MaxValue, 0, GRB.CONTINUOUS, $"x_{i}_{j}");
                    W[i, j] = Modelo.AddVar(0, double.MaxValue, 0, GRB.BINARY, $"w_{i}_{j}");
                }
            }

            for (int i = 0; i < NumOrigens; i++)
            {
                for (int j = 0; j < NumDestinos; j++)
                {
                    FuncaoObjetivo.AddTerm(CustosVariaveis[i, j], X[i, j]);
                    FuncaoObjetivo.AddTerm(CustosFixos[i, j], W[i, j]);
                }
            }
            Modelo.SetObjective(FuncaoObjetivo);

            //Criar as restrições de oferta
            GRBLinExpr expr = new GRBLinExpr();
            for (int i = 0; i < NumOrigens; i++)
            {
                expr.Clear();
                for (int j = 0; j < NumDestinos; j++)
                {
                    expr.AddTerm(1, X[i, j]);
                }
                Modelo.AddConstr(expr <= Ofertas[i], $"Of_{i}");
            }

            //Criar as restrições de demanda
            for (int j = 0; j < NumDestinos; j++)
            {
                expr.Clear();
                for (int i = 0; i < NumOrigens; i++)
                {
                    expr.AddTerm(1, X[i, j]);
                }
                Modelo.AddConstr(expr >= Demandas[j], $"De_{j}");
            }

            //Criar restrições que relacionam as variáveis X e W
            for (int i = 0; i < NumOrigens; i++)
            {
                for (int j = 0; j < NumDestinos; j++)
                {
                    Modelo.AddConstr(X[i, j] <= MatrizM[i, j] * W[i, j], $"Hab_{i}_{j}");
                }
            }

            //Escrever o modelo .lp, resolver o modelo e escrever a solução
            Modelo.Write(@"ModeloTransporte.lp");
            Modelo.Optimize();
            Modelo.Write(@"SolucaoTransporte.sol");
        }

    }
}
