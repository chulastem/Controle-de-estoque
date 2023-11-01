using System.Data.SQLite;
using System.Data;
public class BancoDeDados
{
    //inicia conexao
    private SQLiteConnection connection;

    // Construtor
    public BancoDeDados(string connectionString)
    {
        connection = new SQLiteConnection(connectionString);
    }

    // Método para abrir a conexão
    public void AbrirConexao()
    {
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }
    }
    // Método para obter a conexão
    public SQLiteConnection GetConnection()
    {
        return connection;
    }
    //encerra conexao
    public void FecharBanco()
    {
        if (connection != null)
        {
            connection.Close();
            connection.Dispose();
        }
    }
}