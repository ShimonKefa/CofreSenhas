using System;
using System.Data;
using System.Reflection.Metadata;
using Microsoft.Data.Sqlite;

namespace CofreSenhas
{
    class Program
    {
        static void Main()
        {
            //objeto do banco sem o construtor
            Conexao_Banco? novaConexao = null;
            Console.WriteLine("/home/jotaro/Área de trabalho/");
            Console.WriteLine("Seja bem vindo");
            Console.WriteLine("já possui Banco instalado? (s/n)");
            var aux1 = char.Parse(Console.ReadLine() ?? "n");

            //responsável por pegar o local aonde o banco está instalado/ Pode ser aprimorado se utilizar as variáveis de ambiente do windows, estou no linux.
            Console.WriteLine("Aponte o caminho do banco (Se não existir aponte o local de instalação)");
            string caminhoPasta = Console.ReadLine() ?? " ";
            
            //prossegue com o programa, se não houver banco criado dá erro em produção por não criar a tabela. (A ser corrigido futuramente)
            if(aux1 == 's')
            {
                novaConexao = new Conexao_Banco(caminhoPasta);
                Console.WriteLine("Seja muito bem vindo");
            }
            //caso o banco não tenha sido criado, essa condição vai criar a tabela. O banco é gerado automaticamente aonde ele é apontado pela string _connectionString
            else if(aux1 == 'n')
            {                
                Console.WriteLine("Especifique o local de criação do Banco");
                novaConexao = new Conexao_Banco(caminhoPasta);
                novaConexao.Criar_Tabela_Banco();
                
            }
            else
            {
                Console.WriteLine("operação inválida");
                Environment.Exit(0);
                
            }
            //objeto criado do cofre
            Cofre cofre = new Cofre();
            Console.WriteLine("Selecione a operação :");
            Console.WriteLine("1 - inserir uma nova senha, 2 - listar senhas por serviço, 3 - listar todas as senhas, 4 - Alterar Senha de um serviço, 5 - Deletar serviço");
            var aux2 = Convert.ToInt16(Console.ReadLine());

            switch (aux2)
            {
                //inserir senhas no banco
                case 1:
                    Console.WriteLine("Digite o nome do serviço solicitado: ");
                    string nomeServico = Console.ReadLine() ?? " ";

                    Console.WriteLine("Digite a senha do serviço a ser armazenada: ");
                    string senha = Console.ReadLine() ?? " ";

                    cofre.NomeServico = nomeServico;
                    cofre.Senha = senha;
                    novaConexao.Inserir_Senhas_Banco(cofre);
                break;

                //listar senhas por servico
                case 2:
                    Console.WriteLine("Digite o Serviço da senha que você precisa encontrar");
                    string buscaNome = Console.ReadLine();
                    cofre.NomeServico = buscaNome;
                    novaConexao.Ler_Senhas_Banco(cofre);            
                break;

                //lista todas as senhas do banco
                case 3:
                Console.WriteLine("Listar todas as senhas");
                novaConexao.Listar_Todas_Senhas(cofre);
                break;
                
                //altera uma senha de um serviço
                case 4:
                Console.WriteLine("Digite o ID do servico que você quer alterar");
                int buscaID = int.Parse(Console.ReadLine());
                Console.WriteLine("Digite a nova senha:");
                string novaSenha = Console.ReadLine();
                cofre.ID = buscaID;
                cofre.Senha = novaSenha;
                novaConexao.Alterar_Senha(cofre);
                break;

                case 5:
                Console.WriteLine("Digite o ID da senha a ser deletada");
                buscaID = int.Parse(Console.ReadLine());
                cofre.ID = buscaID;
                novaConexao.Deletar_Servico(cofre);
                break;

                default:
                Console.WriteLine("Operação Inválida");
                break;
            }
        }
    }




    //Essa classe serve como molde dos Dados a serem inseridos dentro do banco. Pode ser aprimorada Futuramente.
    public class Cofre
    {
        public int ID;
        public string Senha { get; set; }
        public string NomeServico { get; set; }
        public DateTime DataServico = DateTime.Now;



    }

    //classe responsável por realizar as conexões com o banco e rodar as querys. 
    public class Conexao_Banco
    {
        private string _defaultPath;
        private string _connectionString;
        public Conexao_Banco(string caminhoPasta)
        {
            _defaultPath = Path.Combine(caminhoPasta, "Cofre.db");
            _connectionString = $"Data Source={_defaultPath}";
        }
      //essa função insere dados no banco. 
      public void Inserir_Senhas_Banco(Cofre cofre)
        {
            using(var DBConnection = new SqliteConnection(_connectionString))
            {
                DBConnection.Open();
                string Query2 = @"
                INSERT INTO senhas (senha, nomeServico, dataServico) VALUES (@senha, @nomeServico, @dataServico)";                 
                 using (var command = new SqliteCommand(Query2, DBConnection))
                {
                    command.Parameters.AddWithValue("@senha", cofre.Senha);
                    command.Parameters.AddWithValue("@nomeServico", cofre.NomeServico);
                    command.Parameters.AddWithValue("@dataServico", cofre.DataServico);

                command.ExecuteNonQuery();
                }
            }
        }
        
        //função utilizada para criar a tabela no banco caso ela não tenha sido criada (A ser aprimorado, Possível redundância). 
        public void Criar_Tabela_Banco()
        {
            using(var DBConnection = new SqliteConnection(_connectionString))
            {
                DBConnection.Open();
                string Query1 = @"
                CREATE TABLE IF NOT EXISTS senhas (
                id integer PRIMARY KEY AUTOINCREMENT,
                senha text NOT NULL,
                nomeServico text NOT NULL,
                dataServico date NOT NULL
                )";

                using (var command = new SqliteCommand(Query1, DBConnection))
                 {
                 command.ExecuteNonQuery();
                 }
            }
        }


        //função lê as senhas dentro do Banco por um parametro nome.
        public void Ler_Senhas_Banco(Cofre cofre)
        {
            using(var DBConnection = new SqliteConnection(_connectionString))
            {
                DBConnection.Open();
                string Query3 = @"
                SELECT nomeServico, senha, dataServico FROM senhas
                WHERE nomeServico = @nomeServico";
                using (var command = new SqliteCommand(Query3, DBConnection))
                {
                    command.Parameters.AddWithValue("@nomeServico", cofre.NomeServico);
                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var _nomeServico = reader.GetString(0);
                        var _senha = reader.GetString(1);
                        var _dataServico = reader.GetString(2);
                        Console.WriteLine($"Nome do Serviço: {_nomeServico}");
                        Console.WriteLine($"Senha: {_senha}");
                        Console.WriteLine($"Data de Criação: {_dataServico}");
                    }
                    
                }
                

            }
        }

        //função para listar todas as senhas
        public void Listar_Todas_Senhas(Cofre cofre)
        {
            using(var DBConnection = new SqliteConnection(_connectionString))
            {
                DBConnection.Open();
                string Query4 = "SELECT * FROM senhas";
                using (var command = new SqliteCommand(Query4, DBConnection))
                {
                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var _id = reader.GetString(0);
                        var _senha = reader.GetString(1);
                        var _nomeServico = reader.GetString(2);
                        var _dataServico = reader.GetString(3);
                        Console.WriteLine($"\n\n\nID do serviço: {_id}");
                        Console.WriteLine($"Nome do Serviço: {_nomeServico}");
                        Console.WriteLine($"Senha: {_senha}");
                        Console.WriteLine($"Data de Criação: {_dataServico}\n\n\n");
                    }   
                }
            }
        }

        //função que altera as senhas dentro do banco
        public void Alterar_Senha(Cofre cofre)
        {
            using(var DBConnection = new SqliteConnection(_connectionString))
            {
                DBConnection.Open();
                string Query5 = @"
                UPDATE senhas 
                SET senha = @senha
                WHERE id = @id";
                using (var command = new SqliteCommand(Query5, DBConnection))
                {
                    command.Parameters.AddWithValue("@id", cofre.ID);
                    command.Parameters.AddWithValue("@senha", cofre.Senha);
                    command.ExecuteNonQuery();
                }
            }
        }
        //Função que deleta senhas no banco por ID
        public void Deletar_Servico(Cofre cofre)
        {
            using(var DBConnection = new SqliteConnection(_connectionString))
            {
                DBConnection.Open();
                string Query6 = @"DELETE FROM senhas WHERE id = @id";
                using (var command = new SqliteCommand(Query6, DBConnection))
                {
                    command.Parameters.AddWithValue("@id", cofre.ID);
                    command.ExecuteNonQuery();
                }
                

            }
        }
    }
}


/*  Coisas a melhorar no código

    ao invés de buscar pelo nome, buscar pelo ID
    arrumar um jeito de não precisar apontar o local da pasta toda vez
    corrigir possíveis falhas
*/
