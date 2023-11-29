using System.Data.SQLite;
using System.Reflection.Metadata;
public class Fornecedor
{
    public string fornecedor, cnpj, contato, endereco, data;
    public Data dt = new Data();
    private SQLiteConnection connection;
    private Random random = new Random();
    public Endereco ed = new Endereco();


    public void Fornecedores(SQLiteConnection dbConnection)
    {
        connection = dbConnection;

        while (true)
        {
            Console.WriteLine("-----| FORNECEDOR |-----\n");
            Console.WriteLine("Escolha uma opção:\n");
            Console.WriteLine("1. Adicionar Fornecedor");
            Console.WriteLine("2. Alterar Fornecedor");
            Console.WriteLine("3. Exibir lista de Fornecedores");
            Console.WriteLine("4. Remover Fornecedor");
            Console.WriteLine("5. Voltar");

            string opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1":
                    Console.Clear();
                    AdicionarFornecedor();
                    break;

                case "2":
                    Console.Clear();
                    AlterarFornecedor();
                    break;
                case "3":
                    Console.Clear();
                    ExibirFornecedor();
                    break;

                case "4":
                    Console.Clear();
                    ExibirFornecedor();
                    RemoverFornecedor();
                    break;

                case "5":
                    Console.Clear();
                    return;

                default:
                    Console.WriteLine("Opção inválida. Tente novamente.\n");
                    break;
            }
        }
    }

    public void AdicionarFornecedor()
    {

        do
        {
            Console.WriteLine("Fornecedor:");
            fornecedor = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(fornecedor))
            {
                Console.WriteLine("Nome do produto não é válido. Insira um nome válido.");
            }
        } while (string.IsNullOrWhiteSpace(fornecedor));

        if (!FornecedorExisteNaTabela(fornecedor))
        {
            do
            {
                Console.WriteLine("Digite o CNPJ ( 00.000.000/0001-00 ):");
                cnpj = Console.ReadLine();

                if (!ValidarCnpj(cnpj))
                {
                    Console.WriteLine("CNPJ inválido. Por favor, insira um CNPJ no formato correto.");
                }
            } while (!ValidarCnpj(cnpj));

            do
            {
                Console.WriteLine("Digite o número de telefone ((99) 9 9999-9999):");
                contato = Console.ReadLine();

                if (!ValidarTelefone(contato))
                {
                    Console.WriteLine("Número de telefone inválido. Por favor, insira um número no formato correto.");
                }
            } while (!ValidarTelefone(contato));

            ed.PreencherEndereco();

            data = dt.DataAtual();

            string sql = "INSERT INTO fornecedor (fornecedor, cnpj, contato, rua, num, bairro, cidade, estado, data_registro) VALUES (@fornecedor, @cnpj, @contato, @rua, @num, @bairro, @cidade, @estado, @data_registro)";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@fornecedor", fornecedor);
                command.Parameters.AddWithValue("@cnpj", cnpj);
                command.Parameters.AddWithValue("@contato", contato);
                command.Parameters.AddWithValue("@rua", ed.Rua);
                command.Parameters.AddWithValue("@num", ed.Num);
                command.Parameters.AddWithValue("@bairro", ed.Bairro);
                command.Parameters.AddWithValue("@cidade", ed.Cidade);
                command.Parameters.AddWithValue("@estado", ed.Estado);
                command.Parameters.AddWithValue("@data_registro", data);
                command.ExecuteNonQuery();
            }
        }
        else
        {
            Console.WriteLine("Você digitou um fornecedor existente.");
        }
    }
    public void ExibirFornecedor()
    {
        string query = "SELECT id, fornecedor, cnpj, contato, rua, num, bairro, cidade, estado, data_registro FROM fornecedor";

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
                    string rua = reader["rua"].ToString();
                    string num = reader["num"].ToString();
                    string bairro = reader["bairro"].ToString();
                    string cidade = reader["cidade"].ToString();
                    string estado = reader["estado"].ToString();
                    string data = reader["data_registro"].ToString();

                    string nEndereco = $"{rua}, {num}, {bairro}, {cidade}, {estado}";

                    Console.WriteLine($"ID: {id}, Fornecedor: {fornecedor}, CNPJ: {cnpj}, Contato: {contato}, Endereço: {nEndereco}, Data de Registo: {data}");
                }
            }
        }
        Console.WriteLine();
    }
    //remove produto
    public void RemoverFornecedor()
    {
        Console.WriteLine("ID do Fornecedor:");
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
            string resetIdSql = "DELETE FROM sqlite_sequence WHERE name = 'fornecedor'";

            using (SQLiteCommand deleteCommand = new SQLiteCommand(deleteSql, connection))
            {
                deleteCommand.Parameters.AddWithValue("@fornecedor", fornecedor);
                deleteCommand.ExecuteNonQuery();

                Console.WriteLine($"'{fornecedor}' foi removido.");
            }
             using (SQLiteCommand resetIdCommand = new SQLiteCommand(resetIdSql, connection))
            {
                resetIdCommand.ExecuteNonQuery();
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
            do
            {
                Console.WriteLine("Novo Fornecedor:");
                novoFornecedor = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(novoFornecedor))
                {
                    Console.WriteLine("Nome do produto não é válido. Insira um nome válido.");
                }
            } while (string.IsNullOrWhiteSpace(novoFornecedor));

            do
            {
                Console.WriteLine("Digite o CNPJ ( 00.000.000/0001-00 ):");
                nCnpj = Console.ReadLine();

                if (!ValidarCnpj(nCnpj))
                {
                    Console.WriteLine("CNPJ inválido. Por favor, insira um CNPJ no formato correto.");
                }
            } while (!ValidarCnpj(nCnpj));
    
            do
            {
                Console.WriteLine("Digite o número de telefone ((99) 9 9999-9999):");
                nContato = Console.ReadLine();

                if (!ValidarTelefone(nContato))
                {
                    Console.WriteLine("Número de telefone inválido. Por favor, insira um número no formato correto.");
                }
            } while (!ValidarTelefone(nContato));

            ed.PreencherEndereco();

            dataAlteracao = dt.DataAtual();

            string sql = "UPDATE fornecedor SET fornecedor = @novoFornecedor, cnpj = @nCnpj, contato = @nContato, rua = @rua, num = @num, bairro = @bairro, cidade = @cidade, estado = @estado, data_registro = @dataAlteracao WHERE fornecedor = @fornecedorAntigo";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@novoFornecedor", novoFornecedor);
                command.Parameters.AddWithValue("@nCnpj", nCnpj);
                command.Parameters.AddWithValue("@nContato", nContato);
                command.Parameters.AddWithValue("@rua", ed.Rua);
                command.Parameters.AddWithValue("@num", ed.Num);
                command.Parameters.AddWithValue("@bairro", ed.Bairro);
                command.Parameters.AddWithValue("@cidade", ed.Cidade);
                command.Parameters.AddWithValue("@estado", ed.Estado);
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
    // tratamento para entrada cnpj
    public bool ValidarCnpj(string cnpj)
    {
        // Verifica se o CNPJ tem o formato correto
        string formatoEsperado = "##.###.###/####-##";
        for (int i = 0; i < cnpj.Length; i++)
        {
            if (formatoEsperado[i] == '#' && !char.IsDigit(cnpj[i]))
            {
                // Caractere esperado é um número, mas encontrou um não número
                return false;
            }
            else if (formatoEsperado[i] != '#' && formatoEsperado[i] != cnpj[i])
            {
                // Caractere esperado é um caractere especial, mas não coincide
                return false;
            }
        }

        return true;
    }
    // tratamento para entrada contato
    public bool ValidarTelefone(string telefone)
    {
        // Verifica se o número de telefone tem o formato correto
        string formatoEsperado = "(##) # ####-####";
        for (int i = 0; i < telefone.Length; i++)
        {
            if (formatoEsperado[i] == '#' && !char.IsDigit(telefone[i]))
            {
                // Caractere esperado é um número, mas encontrou um não número
                return false;
            }
            else if (formatoEsperado[i] != '#' && formatoEsperado[i] != telefone[i])
            {
                // Caractere esperado é um caractere especial, mas não coincide
                return false;
            }
        }

        return true;
    }
    // gera id de 3 algarismos
   
}