using System.Data.SQLite;
public class Fornecedor
{
    public string fornecedor, cnpj, contato, endereco, data;
    public Data dt = new Data();
    private SQLiteConnection connection;

    public void Fornecedores(SQLiteConnection dbConnection)
    {
        connection = dbConnection;

        while (true)
        {
            Console.WriteLine("Escolha uma opção:");
            Console.WriteLine("1. Adicionar Fornecedor");
            Console.WriteLine("2. Alterar Fornecedor");
            Console.WriteLine("3. Exibir lista de Fornecedores");
            Console.WriteLine("4. Remover Fornecedor");
            Console.WriteLine("5. Sair");

            string opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1":
                    AdicionarFornecedor();
                    break;

                case "2":
                    AlterarFornecedor();
                    break;
                case "3":
                    ExibirFornecedor();
                    break;

                case "4":
                    RemoverFornecedor();
                    break;

                case "5":
                    Console.WriteLine("Saindo ...\n");
                    return;

                default:
                    Console.WriteLine("Opção inválida. Tente novamente.\n");
                    break;
            }
        }
    }

    public void AdicionarFornecedor()
    {
        Console.WriteLine("Fornecedor:");
        fornecedor = Console.ReadLine();

        if (!FornecedorExisteNaTabela(fornecedor))
        {


            Console.WriteLine("CNPJ:");
            cnpj = Console.ReadLine();

            Console.WriteLine("Contato:");
            contato = Console.ReadLine();

            Console.WriteLine("Endereço:");
            endereco = Console.ReadLine();

            data = dt.DataAtual();

            string sql = "INSERT INTO fornecedor (fornecedor, cnpj, contato, endereco, data_registro ) VALUES (@fornecedor, @cnpj, @contato, @endereco, @data_registro)";


            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@fornecedor", fornecedor);
                command.Parameters.AddWithValue("@cnpj", cnpj);
                command.Parameters.AddWithValue("@contato", contato);
                command.Parameters.AddWithValue("@endereco", endereco);
                command.Parameters.AddWithValue("@data_registro", data);
                command.ExecuteNonQuery();
            }
        }
        else
        {
            Console.WriteLine("Nenhum registro encontrado com o nome '" + fornecedor + "'. Nenhuma atualização realizada.");
        }
    }
    public void ExibirFornecedor()
    {
        string query = "SELECT id, fornecedor, cnpj, contato, endereco, data_registro FROM fornecedor";

        using (SQLiteCommand command = new SQLiteCommand(query, connection))
        {
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                Console.WriteLine("Itens no Estoque:");
                while (reader.Read())
                {
                    int id = reader.GetInt32(reader.GetOrdinal("id"));
                    string fornecedor = reader["fornecedor"].ToString();
                    string cnpj = reader["cnpj"].ToString();
                    string contato = reader["contato"].ToString();
                    string endereco = reader["endereco"].ToString();
                    string data = reader["data_registro"].ToString();

                    Console.WriteLine($"ID: {id}, Fornecedor: {fornecedor}, CNPJ: {cnpj}, Contato: {contato}, Endereço: {endereco}, Data de Registo: {data}");
                }
            }
        }
    }
    //remove produto
    public void RemoverFornecedor()
    {
        Console.WriteLine("Fornecedor:");
        fornecedor = Console.ReadLine();

        if (FornecedorExisteNaTabela(fornecedor))
        {
            // Consulte o banco de dados para obter o ID do fornecedor a ser removido
            string query = "SELECT id FROM fornecedor WHERE fornecedor = @fornecedor";
            int idFornecedor;

            using (SQLiteCommand selectCommand = new SQLiteCommand(query, connection))
            {
                selectCommand.Parameters.AddWithValue("@fornecedor", fornecedor);
                idFornecedor = Convert.ToInt32(selectCommand.ExecuteScalar());
            }

            // Agora você tem o ID do produto para remoção
            string deleteSql = "DELETE FROM fornecedor WHERE fornecedor = @fornecedor";

            using (SQLiteCommand deleteCommand = new SQLiteCommand(deleteSql, connection))
            {
                deleteCommand.Parameters.AddWithValue("@fornecedor", fornecedor);
                deleteCommand.ExecuteNonQuery();

                // Atualize os IDs dos registros subsequentes (se necessário)
                AtualizarIds(idFornecedor);
                Console.WriteLine($"'{fornecedor}' foi removido.");
            }
        }
        else
        {
            Console.WriteLine("Nenhum registro encontrado com o nome '" + fornecedor + "'. Nenhuma atualização realizada.");
        }
    }
    //altera item
    public void AlterarFornecedor()
    {
        string fornecedorAntigo, novoFornecedor, dataAlteracao, nContato, nEndereco, nCnpj;

        ExibirFornecedor();

        Console.WriteLine("Fornecedor:");
        fornecedorAntigo = Console.ReadLine();

        if (FornecedorExisteNaTabela(fornecedorAntigo))
        {
            Console.WriteLine("Novo Nome:");
            novoFornecedor = Console.ReadLine();

            Console.WriteLine("Novo CNPJ:");
            nCnpj = Console.ReadLine();

            Console.WriteLine("Novo Contato:");
            nContato = Console.ReadLine();

            Console.WriteLine("Novo Endereco:");
            nEndereco = Console.ReadLine();

            dataAlteracao = dt.DataAtual();

            string sql = "UPDATE fornecedor SET fornecedor = @novoFornecedor, cnpj = @nCnpj, contato = @nContato, endereco = @nEndereco, data_registro = @dataAlteracao WHERE fornecedor = @fornecedorAntigo";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@novoFornecedor", novoFornecedor);
                command.Parameters.AddWithValue("@nCnpj", nCnpj);
                command.Parameters.AddWithValue("@nContato", nContato);
                command.Parameters.AddWithValue("@nEndereco", nEndereco);
                command.Parameters.AddWithValue("@dataAlteracao", dataAlteracao);
                command.Parameters.AddWithValue("@fornecedorAntigo", fornecedorAntigo);

                int rowsUpdated = command.ExecuteNonQuery();

                if (rowsUpdated > 0)
                {
                    Console.WriteLine($"{novoFornecedor} foi atualizado");
                }
            }
        }
        else
        {
            Console.WriteLine("Nenhum registro encontrado com o nome '" + fornecedorAntigo + "'. Nenhuma atualização realizada.");
        }
    }
    //verifica se tem o item na tabela
    public bool FornecedorExisteNaTabela(string fornecedor)
    {
        string query = "SELECT COUNT(*) FROM fornecedor WHERE fornecedor = @fornecedor";

        using (SQLiteCommand command = new SQLiteCommand(query, connection))
        {
            command.Parameters.AddWithValue("@fornecedor", fornecedor);
            int rowCount = Convert.ToInt32(command.ExecuteScalar());

            return rowCount > 0;
        }
    }

    public void AtualizarIds(int idFornecedor)
    {
        string updateSql = "UPDATE fornecedor SET id = id - 1 WHERE id > @removedId";

        using (SQLiteCommand updateCommand = new SQLiteCommand(updateSql, connection))
        {
            updateCommand.Parameters.AddWithValue("@removedId", idFornecedor);
            updateCommand.ExecuteNonQuery();
        }
    }
}