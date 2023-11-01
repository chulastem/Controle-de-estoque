public class Data
{
    public string DataAtual()
    {
        DateTime dataAtual = DateTime.Now;

        // Converter a data em uma string no formato padrão
        string dataComoString = dataAtual.ToString();
        // Converter a data em uma string com um formato personalizado
        string formatoPersonalizado = "dd/MM/yyyy";
        string dataFormatada = dataAtual.ToString(formatoPersonalizado);
        return dataFormatada;
    }
}
