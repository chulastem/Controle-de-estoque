using System.Data.SQLite;
using System.Data;
public class Vendas
{
    public List<float> carrinhoValor = new List<float>();
    public List<float> listaValor = new List<float>();
    public List<string> carrinhoProd = new List<string>();
    public List<string> listaProd = new List<string>();
    public List<int> carrinhoQtde = new List<int>();
    public List<int> listaQtde = new List<int>();
    public float troco = 0, totalCompra = 0, valorPago = 0, saldo = 0;
    public int qtde = 0, id = 0, quantidade = 0, carrinho = 3, numeroVenda = 0;
    public string produto, data;
    public float valorUnitario = 0;
    private SQLiteConnection connection;
    public Data dt = new Data();
    public bool carrinhoValido = false, pagamento = false;

    public void RealizarVenda(SQLiteConnection dbConnection)
    {
        connection = dbConnection;

        Console.WriteLine("-----|    REALIZAR VENDA   |-----\n");

        Exibir();

        Console.WriteLine();

        ProdutoVendido();

        string query = "SELECT id, produto, quantidade, valor_unitario, data FROM produto";

        using (SQLiteCommand command = new SQLiteCommand(query, connection))
        {
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                Console.WriteLine("Produtos no Estoque:");
                while (reader.Read())
                {
                    string produto = reader["produto"].ToString();
                    int quantidade = reader.GetInt32(reader.GetOrdinal("quantidade"));
                    float valorUnitario = reader.GetFloat(reader.GetOrdinal("valor_unitario"));

                    listaProd.Add(produto);
                    listaQtde.Add(quantidade);
                    listaValor.Add(valorUnitario);
                }
            }
        }
        query = "SELECT numerovenda FROM caixa ORDER BY numerovenda DESC LIMIT 1;";

        using (SQLiteCommand command = new SQLiteCommand(query, connection))
        {
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    numeroVenda = reader.GetInt32(reader.GetOrdinal("numerovenda"));
                }
                else
                {
                Console.WriteLine("Nenhum resultado encontrado na tabela 'caixa'.");
                }
            }
        }
        do
        {
            Console.WriteLine("Digite o Produto vendido:");
            produto = Console.ReadLine();

            if (listaProd.Contains(produto))
            {
                bool quantidadeValida = false;
                while (!quantidadeValida)
                {
                    Console.WriteLine("Unidades do Produto:");

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

                int indice = listaProd.IndexOf(produto);

                if (quantidade <= listaQtde[indice])
                {
                    CarrinhoCompras(produto, quantidade, listaValor[indice], listaProd);

                    do
                    {
                        Console.WriteLine("Para finalizar compra digite 1\nPara cancelar a compra 2\nPara continuar 3");
                        string inputCarrinho = Console.ReadLine();

                        carrinhoValido = int.TryParse(inputCarrinho, out carrinho) && carrinho >= 1 && carrinho <= 3;
                    
                        if (!carrinhoValido)
                        {
                             Console.WriteLine("Digite uma opção valida: 1, 2 ou 3");
                             Console.WriteLine(carrinho);
                        }
                    }while(!carrinhoValido);

                    if (carrinho == 1)
                    {
                        totalCompra = TotalPagar();
                        Console.WriteLine($"A compra ficou em {totalCompra}.\nDigite o valor Pago.\n");
                        
                        do// do while para confimar se o valor pago e suficiente senao digitar novamente
                        {
                            valorPago = float.Parse(Console.ReadLine());
                            if (valorPago >= totalCompra)
                            {
                                pagamento = true;
                                troco = valorPago - totalCompra; // troco do cliente
                                AlterarSaldo();
                                Console.WriteLine($"O troco é {troco}\n");
                                numeroVenda++;
                                for (int i = 0; i < carrinhoProd.Count; i++)
                                {
                                    quantidade = listaQtde[i] - carrinhoQtde[i];
                                    produto = carrinhoProd[i];
                                    AddHistoricocaixa(i, numeroVenda);
                                    AlterarCompra(produto, quantidade);
                                }
                                break;
                            }
                            else
                            {
                                Console.WriteLine($"Valor insuficiente.\nDigite novamente:\n");
                            }
                        } while (!pagamento);
                    }
                    else if (carrinho == 2)
                    {
                        break; // break para finalizar o codigo se a pessoa digitar 2
                    }
                    else if( carrinho > 3 || carrinho < 1 ) 
                    {
                        Console.WriteLine("Digite uma opção valida");
                        
                    }
                }
                else
                {
                    Console.WriteLine($"Quantidade insuficiente no estoque.\n");
                }
            }
            else
            {
                Console.WriteLine($"Produto '{produto}' não existe.\n");
            }
        } while (carrinho == 3);
    }

    //inicar e alterar saldo por hora inativo
    public void AlterarSaldo()
    {
        string data = dt.DataAtual();
        float valorDoBanco = 0;

        string query = "SELECT valor FROM saldo LIMIT 1;";

        using (SQLiteCommand command = new SQLiteCommand(query, connection))
        {
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    valorDoBanco = Convert.ToSingle(reader["valor"]);
                    Console.WriteLine(valorDoBanco);
                }
                else
                {
                    Console.WriteLine("Nenhum resultado encontrado.");
                }
            }
        }
        Console.WriteLine(valorDoBanco);
        Console.WriteLine(totalCompra);
        totalCompra += valorDoBanco;
        string saldoAtual = "saldo_atual";
        
        string sql = "UPDATE saldo SET saldo = @saldo, valor = @valor, data = @data WHERE saldo = @saldoAntigo";

        using (SQLiteCommand command = new SQLiteCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@saldo", saldoAtual);
            command.Parameters.AddWithValue("@valor", totalCompra);
            command.Parameters.AddWithValue("@data", data);
            command.Parameters.AddWithValue("@saldoAntigo", saldoAtual);

            int rowsUpdated = command.ExecuteNonQuery();

            if (rowsUpdated > 0)
            {
                Console.WriteLine("Saldo foi atualizado");
            }
        }
    }
    public void ExibirSaldo()
    {
        string query = "SELECT valor, data FROM saldo";

        using (SQLiteCommand command = new SQLiteCommand(query, connection))
        {
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    float caixa = reader.GetFloat(reader.GetOrdinal("valor"));
                    string data = reader["data"].ToString();

                    Console.WriteLine($"Caixa: {caixa}, Ultima alteração: {data}");
                }
            }
        }    
    }
    public void ProdutoVendido()
    {

        carrinhoValor.Clear();// zera dicionario
        carrinhoProd.Clear();// zera dicionario
        carrinhoQtde.Clear();// zera dicionario
        listaProd.Clear();// zera dicionario
        listaQtde.Clear();// zera dicionario
        listaValor.Clear();// zera dicionario

        troco = 0; // zera o troco
        saldo =0;
    }

    //adicionar ao carrinho
    public void CarrinhoCompras(string produto, int qtde, float valor, List<string> lista)
    {
        if (lista.Contains(produto))
        {
            carrinhoProd.Add(produto);
            carrinhoQtde.Add(qtde);
            carrinhoValor.Add(valor);
            Console.WriteLine($"{produto} foi adicionado ao carrinho de compras.\n");
        }
        else
        {
            Console.WriteLine("Não tem produto no estoque.");
        }
    }

    // calcula total a pagar
    public float TotalPagar()
    {
        totalCompra = 0; // zera o totalCompra para nao iniciar a funcao com um valor dentro
        for (int i = 0; i < carrinhoProd.Count; i++)
        {
            totalCompra += carrinhoValor[i] * carrinhoQtde[i];
        }
        return totalCompra;
    }

    public void AlterarCompra(string produto, int quantidade)
    {
        string produtoAntigo;
        int novaQuantidade = quantidade;

        produtoAntigo = produto;

        string sql = "UPDATE produto SET quantidade = @novaQuantidade WHERE produto = @produtoAntigo";

        using (SQLiteCommand command = new SQLiteCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@novaQuantidade", novaQuantidade);
            command.Parameters.AddWithValue("@produtoAntigo", produtoAntigo);

            int rowsUpdated = command.ExecuteNonQuery();

            if (rowsUpdated > 0)
            {
                Console.WriteLine($"{produtoAntigo} foi atualizado");
            }
            else
            {
                Console.WriteLine("Nenhum registro encontrado com o nome '" + produtoAntigo + "'. Nenhuma atualização realizada.");
            }
        }
    }
    public void AddHistoricocaixa(int indice, int numeroVenda)
    {
        produto = carrinhoProd[indice];

        quantidade = carrinhoQtde[indice];

        valorUnitario = carrinhoValor[indice];

        float total = quantidade * valorUnitario;

        data = dt.DataAtual();

        string sql = "INSERT INTO caixa (numerovenda, produto, quantidade, valor_unitario, total, data_venda ) VALUES (@numerovenda, @produto, @quantidade, @valor_unitario, @total, @data)";


        using (SQLiteCommand command = new SQLiteCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@numerovenda", numeroVenda);
            command.Parameters.AddWithValue("@produto", produto);
            command.Parameters.AddWithValue("@quantidade", quantidade);
            command.Parameters.AddWithValue("@valor_unitario", valorUnitario);
            command.Parameters.AddWithValue("@total", total);
            command.Parameters.AddWithValue("@data", data);
            command.ExecuteNonQuery();
        }
    }
    public void ExibirHistorico()
    {

        string connectionString = "Data Source=estoque.db;Version=3;";
        connection = new SQLiteConnection(connectionString);
        connection.Open();

        string query = "SELECT numerovenda, produto, quantidade, valor_unitario, total, data_venda FROM caixa";

        using (SQLiteCommand command = new SQLiteCommand(query, connection))
        {
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                Console.WriteLine("-----|   HISTÓRICO DE VENDAS  |-----\n");
                while (reader.Read())
                {
                    int numeroVenda = reader.GetInt32(reader.GetOrdinal("numerovenda"));
                    string produto = reader["produto"].ToString();
                    int quantidade = reader.GetInt32(reader.GetOrdinal("quantidade"));
                    float valorUnitario = reader.GetFloat(reader.GetOrdinal("valor_unitario"));
                    float total = reader.GetFloat(reader.GetOrdinal("total"));
                    string data = reader["data_venda"].ToString();

                    Console.WriteLine($"Numero da venda: {numeroVenda}, Produto: {produto}, Vendidos: {quantidade}, Valor Unitário: {valorUnitario}, total:  {total}, Data da venda: {data}");
                }
            }
        }
        Console.WriteLine();
        ExibirSaldo();
        Console.WriteLine();
        Console.Write("Aperte Enter voltar");
        Console.ReadLine();
        Console.Clear();
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
}