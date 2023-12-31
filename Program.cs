﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.SQLite;
using System.Security.Permissions;

class Program
{
    static void Main()
    {
        string connectionString = "Data Source=estoque.db;Version=3;";
        BancoDeDados bd = new BancoDeDados(connectionString);
        bd.AbrirConexao();

        Produto pd = new Produto();
        Vendas vd = new Vendas();
        Fornecedor fd = new Fornecedor();

        while (true)
        {
            Console.WriteLine("-----| CONTROLE DE ESTOQUE |-----\n");
            Console.WriteLine("Escolha uma opção:\n");
            Console.WriteLine("1. Produtos");
            Console.WriteLine("2. Realizar Venda");
            Console.WriteLine("3. Histórico de Vendas");
            Console.WriteLine("4. Fornecedor");
            Console.WriteLine("5. Sair");

            string opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1":
                    Console.Clear();
                    pd.Produtos(bd.GetConnection());
                    break;

                case "2":
                    Console.Clear();
                    vd.RealizarVenda(bd.GetConnection());
                    break;

                case "3":
                    Console.Clear();
                    vd.ExibirHistorico();
                    break;

                case "4":
                    Console.Clear();
                    fd.Fornecedores(bd.GetConnection());
                    break;

                case "5":
                    Console.Clear();
                    Console.WriteLine("Saindo ...\n");
                    bd.FecharBanco();
                    return;

                default:
                    Console.WriteLine("Opção inválida. Tente novamente.\n");
                    break;
            }
        }
    }
}
