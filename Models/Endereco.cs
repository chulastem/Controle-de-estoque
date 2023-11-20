using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class Endereco
{
    public string Rua { get; set; }
    public string Num { get; set; }
    public string Bairro { get; set; }
    public string Cidade { get; set; }
    public string Estado { get; set; }

    public Endereco() { }
    public bool condicao = false;

    public void PreencherEndereco()
    {
        do
        {
            Console.Write("Rua: ");
            string rua = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(rua))
            {
                Rua = rua;
                condicao = true;
            }
        }while(!condicao);

        condicao = false;
        do
        {
            Console.Write("Número: ");
            string numeroInput = Console.ReadLine();
            if (int.TryParse(numeroInput, out int numero))
            {
                Num = numero.ToString();
                condicao = true;
            }
        }while(!condicao);

        condicao = false;
        do
        {
            Console.Write("Bairro: ");
            string bairro = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(bairro))
            {
                Bairro = bairro;
                condicao = true;
            }
        }while(!condicao);

        condicao = false;
        do
        {
            Console.Write("Cidade: ");
            string cidade = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(cidade))
            {
                Cidade = cidade;
                condicao = true;
            }
        }while(!condicao);

        condicao = false;
        do
        {
            Console.Write("Estado: ");
            string estado = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(estado))
            {
                Estado = estado;
                condicao = true;
            }
        }while(!condicao);
    }
}