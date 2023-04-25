using Newtonsoft.Json;
using Npgsql;
using System;
using System.Data;
using System.Net.Http;
using System.Threading.Tasks;

bool enderecoEncontrado = true;

while (enderecoEncontrado)
{
    Console.WriteLine("CEP: ");
    string? cep = Console.ReadLine();

    using (var httpClient = new HttpClient())
    {
        var response = httpClient.GetAsync($"https://viacep.com.br/ws/{cep}/json/").Result;
        if (response.IsSuccessStatusCode)
        {
            var content = response.Content.ReadAsStringAsync().Result;
            var endereco = JsonConvert.DeserializeObject<Endereco>(content);
            if (endereco != null && !string.IsNullOrEmpty(endereco.CEP))
            {
                Console.WriteLine("Endereço encontrado.");
                Console.WriteLine($"CEP: {cep}");
                Console.WriteLine($"Logradouro: {endereco.Logradouro}");
                Console.WriteLine($"Complemento: {endereco.Complemento}");
                Console.WriteLine($"Bairro: {endereco.Bairro}");
                Console.WriteLine($"Localidade: {endereco.Localidade}");
                Console.WriteLine($"UF: {endereco.UF}");
                Console.WriteLine($"IBGE: {endereco.IBGE}");
                Console.WriteLine($"GIA: {endereco.GIA}");
                Console.WriteLine($"DDD: {endereco.DDD}");
                Console.WriteLine($"SIAFI: {endereco.SIAFI}");

                bool registroExistente = VerificarRegistroExistenteNoBancoDeDados(endereco.CEP);

                if (registroExistente)

                {
                    AtualizarEnderecoNoBancoDeDados(endereco);
                    Console.WriteLine("Endereço atualizado no banco de dados.");
                }
            
            else
            {
                InserirEnderecoNoBancoDeDados(endereco);
                Console.WriteLine("Endereço inserido no banco de dados.");
            }

                Console.WriteLine("Deseja excluir o endereço? (S/N)");
                string opcaoExclusao = Console.ReadLine();
                if (opcaoExclusao.ToUpper() == "S")
                {
                    RemoverEnderecoDoBancoDeDados(endereco.CEP);
                    Console.WriteLine("Endereço excluído do banco de dados.");
                }

                enderecoEncontrado = false;
            }
            else
            {
                Console.WriteLine("Endereco não encontrado. Por favor, insira o CEP novamente.");
            }
        }
        else
        {
            Console.WriteLine($"Erro. Por favor, insira o CEP novamente.");
        }
    }
}

void InserirEnderecoNoBancoDeDados(Endereco endereco)
{
    using (var connection = new NpgsqlConnection("Host=localhost:5432;Username=postgres;Password=1478951;Database=postgres"))
    {
        connection.Open();

        using (var command = new NpgsqlCommand("INSERT INTO endereco (cep, logradouro, complemento, bairro, localidade, uf, ibge, gia, ddd, siafi) " +
                                               "VALUES (@cep, @logradouro, @complemento, @bairro, @localidade, @uf, @ibge, @gia, @ddd, @siafi)", connection))
        {
            command.Parameters.AddWithValue("@cep", endereco?.CEP);
            command.Parameters.AddWithValue("@logradouro", endereco?.Logradouro);
            command.Parameters.AddWithValue("@complemento", endereco?.Complemento);
            command.Parameters.AddWithValue("@bairro", endereco?.Bairro);
            command.Parameters.AddWithValue("@localidade", endereco?.Localidade);
            command.Parameters.AddWithValue("@uf", endereco?.UF);
            command.Parameters.AddWithValue("@ibge", endereco?.IBGE);
            command.Parameters.AddWithValue("@gia", endereco?.GIA);
            command.Parameters.AddWithValue("@ddd", endereco?.DDD);
            command.Parameters.AddWithValue("@siafi", endereco?.SIAFI);

            command.ExecuteNonQuery();
        }
    }
}

void AtualizarEnderecoNoBancoDeDados(Endereco endereco)
{
    using (var connection = new NpgsqlConnection("Host=localhost:5432;Username=postgres;Password=1478951;Database=postgres"))
    {
        connection.Open();

        using (var command = new NpgsqlCommand("UPDATE endereco SET logradouro=@logradouro, complemento=@complemento, " +
                                               "bairro=@bairro, localidade=@localidade, uf=@uf, ibge=@ibge, gia=@gia, ddd=@ddd, siafi=@siafi " +
                                               "WHERE cep=@cep", connection))
        {
            command.Parameters.AddWithValue("@logradouro", endereco?.Logradouro);
            command.Parameters.AddWithValue("@complemento", endereco?.Complemento);
            command.Parameters.AddWithValue("@bairro", endereco?.Bairro);
            command.Parameters.AddWithValue("@localidade", endereco?.Localidade);
            command.Parameters.AddWithValue("@uf", endereco?.UF);
            command.Parameters.AddWithValue("@ibge", endereco?.IBGE);
            command.Parameters.AddWithValue("@gia", endereco?.GIA);
            command.Parameters.AddWithValue("@ddd", endereco?.DDD);
            command.Parameters.AddWithValue("@siafi", endereco?.SIAFI);
            command.Parameters.AddWithValue("@cep", endereco?.CEP);

            command.ExecuteNonQuery();
        }
    }
}

void RemoverEnderecoDoBancoDeDados(string cep)
{
    using (var connection = new NpgsqlConnection("Host=localhost:5432;Username=postgres;Password=1478951;Database=postgres"))
    {
        connection.Open();

        using (var command = new NpgsqlCommand("DELETE FROM endereco WHERE cep=@cep", connection))
        {
            command.Parameters.AddWithValue("@cep", cep);

            command.ExecuteNonQuery();
        }
    }
}

bool VerificarRegistroExistenteNoBancoDeDados(string cep)
{
    using (var connection = new NpgsqlConnection("Host=localhost:5432;Username=postgres;Password=1478951;Database=postgres"))
    {
        connection.Open();

        using (var command = new NpgsqlCommand("SELECT COUNT(*) FROM endereco WHERE cep=@cep", connection))
        {
            command.Parameters.AddWithValue("@cep", cep);

            int count = Convert.ToInt32(command.ExecuteScalar());

            return count > 0;
        }
    }
}

class Endereco
{
    public string? CEP { get; set; }
    public string? Logradouro { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Localidade { get; set; }
    public string? UF { get; set; }
    public string? IBGE { get; set; }
    public string? GIA { get; set; }
    public string? DDD { get; set; }
    public string? SIAFI { get; set; }
}
