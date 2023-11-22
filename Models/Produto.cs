using System.ComponentModel;
using System.Data.Common;
using System.Data.SQLite;
using System.Collections.Generic;
public class Produto
{
    public string produto, dataValidade;
    public int quantidade = 0;
    public float valorUnitario = 0;
    private BancoDeDados db;
    private SQLiteConnection connection;
    private Random random = new Random();
    public int id;

    public void Produtos(SQLiteConnection dbConnection)
    {
        connection = dbConnection;

        while (true)
        {
            Console.WriteLine("Escolha uma opção:");
            Console.WriteLine("1. Adicionar produto");
            Console.WriteLine("2. Alterar produto");
            Console.WriteLine("3. Exibir lista de produtos");
            Console.WriteLine("4. Remover produto");
            Console.WriteLine("5. Voltar");

            string opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1":
                    AdicionarProduto();
                    break;

                case "2":
                    AlterarProduto();
                    break;
                case "3":
                    Exibir();
                    break;

                case "4":
                    RemoverProduto();
                    break;

                case "5":
                    return;

                default:
                    Console.WriteLine("Opção inválida. Tente novamente.\n");
                    break;
            }
        }
    }

    //adiciona item
    public void AdicionarProduto()
    {

        do
        {
            Console.WriteLine("Produto:");
            produto = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(produto))
            {
                Console.WriteLine("Nome do produto não é válido. Insira um nome válido.");
            }
        } while (string.IsNullOrWhiteSpace(produto));

        if (!ProdutoExisteNaTabela(produto))
        {
            bool quantidadeValida = false;
            while (!quantidadeValida)
            {
                Console.WriteLine("Quantidade:");

                if (int.TryParse(Console.ReadLine(), out quantidade))
                {
                    if (quantidade >= 0)
                    {
                        quantidadeValida = true;
                    }
                    else
                    {
                        Console.WriteLine("Quantidade deve ser maior que zero.");
                    }
                }
                else
                {
                    Console.WriteLine("Quantidade inválida. Insira um número inteiro válido.");
                }
            }

            bool valorValido = false;
            while (!valorValido)
            {
                Console.WriteLine("Digite o Valor Unitário:");

                // Tenta converter a entrada para float
                if (float.TryParse(Console.ReadLine(), out valorUnitario))
                {
                    if (valorUnitario >= 0)
                    {
                        valorValido = true;
                    }
                    else
                    {
                        Console.WriteLine("Valor deve ser maior que zero.");
                    }
                }
                else
                {
                    Console.WriteLine("Por favor, digite um valor numérico válido.");
                }
            }

            do
            {
                Console.WriteLine("Digite a Data de Validade (no formato dd/mm/yyyy):");

                // Recebe a entrada do usuário
                dataValidade = Console.ReadLine();

                // Verifica se a entrada tem o formato correto e contém apenas números
                if (ValidarDataValidade(dataValidade))
                {
                    // Saímos do loop se a entrada for válida
                    break;
                }
                else
                {
                    Console.WriteLine("Formato de data inválido. Por favor, tente novamente.");
                }

            } while (true);

            string sql = "INSERT INTO produto (produto, quantidade, valor_unitario, data ) VALUES (@produto, @quantidade, @valor_unitario, @data)";


            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@produto", produto);
                command.Parameters.AddWithValue("@quantidade", quantidade);
                command.Parameters.AddWithValue("@valor_unitario", valorUnitario);
                command.Parameters.AddWithValue("@data", dataValidade);
                command.ExecuteNonQuery();
            }
        }
        else
        {
            Console.WriteLine("Ja existe um  registro com o nome '" + produto + "'. Digite outro produto.");
        }
    }
    //Mostra todos os itens da tabela produto
    public void Exibir()
    {

        string query = "SELECT id, produto, quantidade, valor_unitario, data FROM produto";

        using (SQLiteCommand command = new SQLiteCommand(query, connection))
        {
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                Console.WriteLine("Itens no Estoque:");
                while (reader.Read())
                {
                    int id = reader.GetInt32(reader.GetOrdinal("id"));
                    string produto = reader["produto"].ToString();
                    int quantidade = reader.GetInt32(reader.GetOrdinal("quantidade"));
                    float valorUnitario = reader.GetFloat(reader.GetOrdinal("valor_unitario"));
                    string dataValidade = reader["data"].ToString();

                    Console.WriteLine($"ID: {id}, Produto: {produto}, Quantidade: {quantidade}, Valor Unitário: {valorUnitario}, Data de Validade: {dataValidade}");
                }
            }
        }
    }
    //remove produto da tabela
    public void RemoverProduto()
    {
        Console.WriteLine("Digite o ID ou o nome do produto");
        produto = Console.ReadLine();

        if (int.TryParse(produto, out int id))
        {
            // Se o usuário digitou um número, remover pelo ID
            RemoverProdutoPorId(id);
        }
        else
        {
            // Se o usuário não digitou um número, tentar remover pelo nome do produto
            RemoverProdutoPorNome(produto);
        }

    }

    public void RemoverProdutoPorNome(string produto)
    {
        if (ProdutoExisteNaTabela(produto))
        {
            string query = "SELECT id FROM produto WHERE produto = @produto";
            int idDoProdutoParaRemover;

            using (SQLiteCommand selectCommand = new SQLiteCommand(query, connection))
            {
                selectCommand.Parameters.AddWithValue("@produto", produto);
                idDoProdutoParaRemover = Convert.ToInt32(selectCommand.ExecuteScalar());
            }

            string deleteSql = "DELETE FROM produto WHERE produto = @produto";
            string resetIdSql = "DELETE FROM sqlite_sequence WHERE name = 'produto'";

            using (SQLiteCommand deleteCommand = new SQLiteCommand(deleteSql, connection))
            {
                deleteCommand.Parameters.AddWithValue("@produto", produto);
                deleteCommand.ExecuteNonQuery();

                Console.WriteLine($"Produto '{produto}' foi removido.");
            }
            using (SQLiteCommand resetIdCommand = new SQLiteCommand(resetIdSql, connection))
            {
                resetIdCommand.ExecuteNonQuery();
            }
        }
        else
        {
            Console.WriteLine($"Nenhum produto encontrado com o nome '{produto}'. Nenhuma atualização realizada.");
        }
    }

    public void RemoverProdutoPorId(int id)
{
    string deleteSql = "DELETE FROM produto WHERE id = @id";
    string resetIdSql = "DELETE FROM sqlite_sequence WHERE name = 'produto'";

    using (SQLiteCommand deleteCommand = new SQLiteCommand(deleteSql, connection))
    {
        deleteCommand.Parameters.AddWithValue("@id", id);
        int rowsAffected = deleteCommand.ExecuteNonQuery();

        if (rowsAffected > 0)
        {
            Console.WriteLine($"Produto com ID {id} foi removido.");
        }
        else
        {
            Console.WriteLine($"Nenhum produto encontrado com o ID {id}. Nenhuma atualização realizada.");
        }
    }
     using (SQLiteCommand resetIdCommand = new SQLiteCommand(resetIdSql, connection))
            {
                resetIdCommand.ExecuteNonQuery();
            }
}



    //altera item
    public void AlterarProduto()
    {
        // iniciando variaveis
        string produtoAntigo, novoProduto, novaData;
        int novaQuantidade = 0;
        float novoValor = 0;

        Exibir();

        Console.WriteLine("Digite o produto que deseja alterar:");
        produtoAntigo = Console.ReadLine();

        if (ProdutoExisteNaTabela(produtoAntigo))//verifica se tem produto na tabela produto
        {
            //loop para tratamento do produto
            do
            {
                Console.WriteLine("Produto:");
                novoProduto = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(novoProduto))
                {
                    Console.WriteLine("Nome do produto não é válido. Insira um nome válido.");
                }
            } while (string.IsNullOrWhiteSpace(novoProduto));

            // loop para digitar quantidade valida
            bool quantidadeValida = false;
            while (!quantidadeValida)
            {
                Console.WriteLine("Quantidade:");

                if (int.TryParse(Console.ReadLine(), out novaQuantidade))
                {
                    if (novaQuantidade >= 0)
                    {
                        quantidadeValida = true;
                    }
                    else
                    {
                        Console.WriteLine("Quantidade deve ser maior que zero.");
                    }
                }
                else
                {
                    Console.WriteLine("Quantidade inválida. Insira um número inteiro válido.");
                }
            }

            //loop para digitar valor adequado
            bool valorValido = false;
            while (!valorValido)
            {
                Console.WriteLine("Digite o Valor Unitário:");

                // Tenta converter a entrada para float
                if (float.TryParse(Console.ReadLine(), out novoValor))
                {
                    if (novoValor >= 0)
                    {
                        valorValido = true;
                    }
                    else
                    {
                        Console.WriteLine("Valor deve ser maior que zero.");
                    }
                }
                else
                {
                    Console.WriteLine("Por favor, digite um valor numérico válido.");
                }
            }

            // loop para usuario digitar a data adequada
            do
            {
                Console.WriteLine("Digite a Data de Validade (no formato dd/mm/yyyy):");

                // Recebe a entrada do usuário
                novaData = Console.ReadLine();

                // Verifica se a entrada tem o formato correto e contém apenas números
                if (ValidarDataValidade(novaData))
                {
                    // Saímos do loop se a entrada for válida
                    break;
                }
                else
                {
                    Console.WriteLine("Formato de data inválido. Por favor, tente novamente.");
                }

            } while (true);

            string sql = "UPDATE produto SET produto = @novoProduto, quantidade = @novaQuantidade, valor_unitario = @novoValor, data = @novaData WHERE produto = @produtoAntigo";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@novoProduto", novoProduto);
                command.Parameters.AddWithValue("@novaQuantidade", novaQuantidade);
                command.Parameters.AddWithValue("@novoValor", novoValor);
                command.Parameters.AddWithValue("@novaData", novaData);
                command.Parameters.AddWithValue("@produtoAntigo", produtoAntigo);

                int rowsUpdated = command.ExecuteNonQuery();

                if (rowsUpdated > 0)
                {
                    Console.WriteLine($"{novoProduto} foi atualizado");
                }
            }
        }
        else
        {
            Console.WriteLine("Nenhum registro encontrado com o nome '" + produtoAntigo + "'. Nenhuma atualização realizada.");
        }
    }
    //verifica se tem o item na tabela
    public bool ProdutoExisteNaTabela(string produto)
    {
        string query = "SELECT COUNT(*) FROM produto WHERE produto = @produto";

        using (SQLiteCommand command = new SQLiteCommand(query, connection))
        {
            command.Parameters.AddWithValue("@produto", produto);
            int rowCount = Convert.ToInt32(command.ExecuteScalar());

            return rowCount > 0;
        }
    }
    //tratamento de entrada para data de validade
    public bool ValidarDataValidade(string data)
    {
        // Tenta converter a entrada para DateTime
        if (System.Text.RegularExpressions.Regex.IsMatch(data, @"^\d{2}/\d{2}/\d{4}$"))
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    //verifica se ja tem id na tabela produto
    // public bool IdExisteNaTabela(int id)
    // {
    //     string query = "SELECT COUNT(*) FROM produto WHERE id = @id";

    //     using (SQLiteCommand command = new SQLiteCommand(query, connection))
    //     {
    //         command.Parameters.AddWithValue("@id", id);
    //         int rowCount = Convert.ToInt32(command.ExecuteScalar());

    //         return rowCount > 0;
    //     }
    // }
}