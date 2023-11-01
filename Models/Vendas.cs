using System.Data.SQLite;
public class Vendas
{
    private BancoDeDados db;
    public List<float> carrinhoValor = new List<float>();
    public List<float> listaValor = new List<float>();
    public List<string> carrinhoProd = new List<string>();
    public List<string> listaProd = new List<string>();
    public List<int> carrinhoQtde = new List<int>();
    public List<int> listaQtde = new List<int>();
    public float troco = 0, totalCompra = 0, valorPago = 0, saldo = 0;
    public int qtde = 0, id = 0, quantidade = 0, carrinho = 3, numeroVenda = 0;
    public string produto, dataValidade, data;
    public float valorUnitario = 0;
    private SQLiteConnection connection;
    public Data dt = new Data();

    public void RealizarVenda(SQLiteConnection dbConnection)
    {
        connection = dbConnection;

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

        do
        {
            Console.WriteLine("Digite o Produto vendido:");
            produto = Console.ReadLine();

            if (listaProd.Contains(produto))
            {
                Console.WriteLine("Unidades do produto: ");
                quantidade = int.Parse(Console.ReadLine());
                int indice = listaProd.IndexOf(produto);

                if (quantidade <= listaQtde[indice])
                {
                    CarrinhoCompras(produto, quantidade, listaValor[indice], listaProd);
                    Console.WriteLine("Para finalizar compra digite 1\nPara cancelar a compra 2\nPara continuar 3");
                    carrinho = int.Parse(Console.ReadLine());

                    if (carrinho == 1)
                    {
                        totalCompra = TotalPagar();
                        Console.WriteLine($"A compra ficou em {totalCompra}.\nDigite o valor Pago.\n");
                        do// do while para confimar se o valor pago e suficiente senao digitar novamente
                        {
                            valorPago = float.Parse(Console.ReadLine());
                            if (valorPago >= totalCompra)
                            {
                                troco = valorPago - totalCompra; // troco do cliente
                                saldo += valorPago; // adiciona no caixa o valor da venda
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
                        } while (false);
                    }
                    else if (carrinho == 2)
                    {
                        break; // break para finalizar o codigo se a pessoa digitar 2
                    }
                }
                else
                {
                    Console.WriteLine($"Produto '{produto}' não existe.\n");
                }
            }
            else
            {
                Console.WriteLine($"Quantidade insuficiente no estoque.\n");
            }
        } while (carrinho == 3);
    }

    //inicar e alterar saldo por hora inativo
    public void IniciarAlterarSaldo()
    {
        Console.WriteLine($"Digite o saldo do caixa");
        saldo = float.Parse(Console.ReadLine());
    }

    //realiza o cadastro de cada protudo comprado e adiciona no carrinho
    public void ProdutoVendido()
    {

        carrinhoValor.Clear();// zera dicionario
        carrinhoProd.Clear();// zera dicionario
        carrinhoQtde.Clear();// zera dicionario
        listaProd.Clear();// zera dicionario
        listaQtde.Clear();// zera dicionario
        listaValor.Clear();// zera dicionario

        troco = 0; // zera o troco
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
                Console.WriteLine("Itens no Estoque:");
                while (reader.Read())
                {
                    int numeroVenda = reader.GetInt32(reader.GetOrdinal("numerovenda"));
                    string produto = reader["produto"].ToString();
                    int quantidade = reader.GetInt32(reader.GetOrdinal("quantidade"));
                    float valorUnitario = reader.GetFloat(reader.GetOrdinal("valor_unitario"));
                    float total = reader.GetFloat(reader.GetOrdinal("total"));
                    string data = reader["data_venda"].ToString();

                    Console.WriteLine($"ID: {numeroVenda}, Produto: {produto}, Quantidade: {quantidade}, Valor Unitário: {valorUnitario}, Total {total}, Data de Validade: {data}");
                }
            }
        }
    }
}