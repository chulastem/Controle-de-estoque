using System.Data.Common;
using System.Data.SQLite;
public class Produto
{
    public string produto, dataValidade;
    public int quantidade = 0;
    bool quantidadeValida = false;
    public float valorUnitario = 0;
    private BancoDeDados db;
    private SQLiteConnection connection;

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


        if (!ProdutoExisteNaTabela(produto))
        {
            do
            {
                Console.WriteLine("Produto:");
                produto = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(produto) || string.IsNullOrEmpty(produto))
                {
                    Console.WriteLine("Nome do produto não é válido. Insira um nome válido.");
                }
            } while (string.IsNullOrWhiteSpace(produto));

            while (!quantidadeValida)
            {
                Console.WriteLine("Quantidade:");
                string input = Console.ReadLine();

                if (int.TryParse(input, out quantidade))
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


            Console.WriteLine("Valor Unitario:");
            valorUnitario = float.Parse(Console.ReadLine());
            

            Console.WriteLine("Data de Validade:");
            dataValidade = Console.ReadLine();

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
            Console.WriteLine("Nenhum registro encontrado com o nome '" + produto + "'. Nenhuma atualização realizada.");
        }
    }
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
    //remove produto
    public void RemoverProduto()
    {
        Console.WriteLine("Produto:");
        produto = Console.ReadLine();

        if (ProdutoExisteNaTabela(produto))
        {
            // Consulte o banco de dados para obter o ID do produto a ser removido
            string query = "SELECT id FROM produto WHERE produto = @produto";
            int idDoProdutoParaRemover;

            using (SQLiteCommand selectCommand = new SQLiteCommand(query, connection))
            {
                selectCommand.Parameters.AddWithValue("@produto", produto);
                idDoProdutoParaRemover = Convert.ToInt32(selectCommand.ExecuteScalar());
            }

            // Agora você tem o ID do produto para remoção
            string deleteSql = "DELETE FROM produto WHERE produto = @produto";

            using (SQLiteCommand deleteCommand = new SQLiteCommand(deleteSql, connection))
            {
                deleteCommand.Parameters.AddWithValue("@produto", produto);
                deleteCommand.ExecuteNonQuery();

                // Atualize os IDs dos registros subsequentes (se necessário)
                AtualizarIds(idDoProdutoParaRemover);
                Console.WriteLine($"'{produto}' foi removido.");
            }
        }
        else
        {
            Console.WriteLine("Nenhum registro encontrado com o nome '" + produto + "'. Nenhuma atualização realizada.");
        }
    }
    //altera item
    public void AlterarProduto()
    {
        string produtoAntigo, novoProduto, novaData;
        int novaQuantidade = 0;
        float novoValor = 0;

        Exibir();

        Console.WriteLine("Produto:");
        produtoAntigo = Console.ReadLine();

        if (ProdutoExisteNaTabela(produtoAntigo))
        {
            Console.WriteLine("Novo nome:");
            novoProduto = Console.ReadLine();

            Console.WriteLine("Nova quantidade:");
            novaQuantidade = int.Parse(Console.ReadLine());

            Console.WriteLine("Novo Valor:");
            novoValor = float.Parse(Console.ReadLine());

            Console.WriteLine("Nova Data de Validade:");
            novaData = Console.ReadLine();

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
    public void AtualizarIds(int idDoProdutoParaRemover)
    {
        string updateSql = "UPDATE produto SET id = id - 1 WHERE id > @removedId";

        using (SQLiteCommand updateCommand = new SQLiteCommand(updateSql, connection))
        {
            updateCommand.Parameters.AddWithValue("@removedId", idDoProdutoParaRemover);
            updateCommand.ExecuteNonQuery();
        }
    }
}