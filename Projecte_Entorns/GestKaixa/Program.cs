using System;
using MySql.Data.MySqlClient;

namespace GestKaixa
{
    class Program
    {
        static string connectionString = "Server=10.2.54.153;Database=kaixa;Uid=cashbox_app;Pwd=app123;";
        static int usuariId = 0;

        static void Main(string[] args)
        {
            FuncioPrincipal();
        }

        static void FuncioPrincipal()
        {
            bool esAdmin;
            UsuariIValidar(out esAdmin);
            if (esAdmin)
            {
                GestionarMenuAdmin();
            }
            else if (usuariId != 0)
            {
                GestionarMenuClient();
            }
        }

        static void UsuariIValidar(out bool esAdmin)
        {
            Console.WriteLine("BENVINGUT A GESTKAIXA");
            Console.Write("Usuari: ");
            string usuari = Console.ReadLine();
            Console.Write("Contrasenya: ");
            string password = Console.ReadLine();

            if (usuari == "cashbox_app" && password == "app123")
            {
                esAdmin = true;
            }
            else if (ValidarClientABaseDeDades(usuari, password))
            {
                esAdmin = false;
            }
            else
            {
                Console.WriteLine("Accés denegat. Credencials incorrectes.");
                esAdmin = false;
            }
        }

        static bool ValidarClientABaseDeDades(string usuari, string password)
        {
            bool loginCorrecte = false;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT id FROM Usuaris WHERE username = @user AND password = @pass";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", usuari);
                        cmd.Parameters.AddWithValue("@pass", password);
                        object resultat = cmd.ExecuteScalar();
                        if (resultat != null)
                        {
                            usuariId = Convert.ToInt32(resultat);
                            loginCorrecte = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error de BD: " + ex.Message);
                }
            }
            return loginCorrecte;
        }

        static int MostrarMenuAdmin()
        {
            int opcio;
            Console.WriteLine("SISTEMA GESTKAIXA - MÒDUL ADMINISTRADOR");
            Console.WriteLine("1. Registrar nous usuaris");
            Console.WriteLine("2. Obrir nous comptes.");
            Console.WriteLine("3. Assignar usuaris a comptes.");
            Console.WriteLine("4. Llistar tots els usuaris i els comptes associats.");
            Console.WriteLine("5. Sortir");
            opcio = Convert.ToInt32(Console.ReadLine());
            return opcio;
        }

        static int MostrarMenuClient()
        {
            int opcio;
            Console.WriteLine("1. Veure els seus comptes.");
            Console.WriteLine("2. Consultar el saldo.");
            Console.WriteLine("3. Veure moviments.");
            Console.WriteLine("4. Fer un ingrés.");
            Console.WriteLine("5. Fer una retirada.");
            Console.WriteLine("6. Veure alertes");
            Console.WriteLine("7. Sortir");
            opcio = Convert.ToInt32(Console.ReadLine());
            return opcio;
        }

        static void GestionarMenuAdmin()
        {
            int opcio = 0;
            do
            {
                opcio = MostrarMenuAdmin();
                switch (opcio)
                {
                    case 1: RegistrarNouUsuari(); break;
                    case 2: ObrirNouCompte(); break;
                    case 3: AssignarUsuariACompte(); break;
                    case 4: LlistarUsuarisIComptes(); break;
                    case 5: Console.WriteLine("Sortint..."); break;
                    default: Console.WriteLine("Opció no vàlida."); break;
                }
            } while (opcio != 5);
        }

        static void GestionarMenuClient()
        {
            int opcio = 0;
            do
            {
                opcio = MostrarMenuClient();
                switch (opcio)
                {
                    case 1: VeureComptes(); break;
                    case 2: ConsultarSaldo(); break;
                    case 3: VeureMoviments(); break;
                    case 4: FerIngres(); break;
                    case 5: FerRetirada(); break;
                    case 6: VeureAlertes(); break;
                    case 7: Console.WriteLine("Ha sortit"); break;
                    default: Console.WriteLine("Opció no vàlida."); break;
                }
            } while (opcio != 7);
        }

        static void VeureComptes()
        {
            Console.WriteLine(" COMPTES ASSOCIATS ");
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string sql = @"SELECT c.numero_compte, c.estat, c.data_creacio 
                                   FROM Comptes c
                                   INNER JOIN UsuarisComptes uc ON c.id = uc.compte_id
                                   WHERE uc.usuari_id = @uid";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", usuariId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                Console.WriteLine($"Número: {reader["numero_compte"]} | Estat: {reader["estat"]} | Data: {reader["data_creacio"]}");
                        }
                    }
                }
                catch (Exception ex) { Console.WriteLine("Error de BD: " + ex.Message); }
            }
            Console.WriteLine("\nPrem Enter per continuar...");
            Console.ReadLine();
        }

        static void ConsultarSaldo()
        {

            Console.WriteLine("CONSULTA DE SALDO: ");
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string sql = @"SELECT c.numero_compte, vs.saldo 
                                   FROM VistaSaldos vs
                                   INNER JOIN Comptes c ON vs.compte_id = c.id
                                   INNER JOIN UsuarisComptes uc ON c.id = uc.compte_id
                                   WHERE uc.usuari_id = @uid";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", usuariId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                Console.WriteLine($"Compte: {reader["numero_compte"]} | Saldo: {reader["saldo"]} €");
                        }
                    }
                }
                catch (Exception ex) { Console.WriteLine("Error de BD: " + ex.Message); }
            }
            Console.WriteLine("\nPrem Enter per continuar...");
            Console.ReadLine();
        }

        static void VeureMoviments()
        {

            Console.WriteLine(" MOVIMENTS: ");
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string sql = @"SELECT m.data, m.concepte, m.import, m.saldo, c.numero_compte
                                   FROM Moviments m
                                   INNER JOIN Comptes c ON m.compte_id = c.id
                                   INNER JOIN UsuarisComptes uc ON c.id = uc.compte_id
                                   WHERE uc.usuari_id = @uid
                                   ORDER BY m.data DESC";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", usuariId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                Console.WriteLine($"[{reader["data"]}] {reader["numero_compte"]} | {reader["concepte"]} | Import: {reader["import"]} € | Saldo: {reader["saldo"]} €");
                        }
                    }
                }
                catch (Exception ex) { Console.WriteLine("Error de BD: " + ex.Message); }
            }
            Console.WriteLine("\nPrem Enter per continuar...");
            Console.ReadLine();
        }

        static void FerIngres()
        {
            Console.WriteLine(" FER UN INGRÉS: ");
            Console.Write("Número de compte: "); string numeroCompte = Console.ReadLine();
            Console.Write("Import: "); decimal import = Convert.ToDecimal(Console.ReadLine());
            Console.Write("Concepte: "); string concepte = Console.ReadLine();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    int compteId = ObtenirCompteId(conn, numeroCompte);
                    if (compteId == 0) { Console.WriteLine("Compte no trobat o no autoritzat."); return; }
                    decimal nouSaldo = ObtenirSaldo(conn, compteId) + import;
                    InserirMoviment(conn, compteId, import, concepte, nouSaldo);
                    Console.WriteLine($"Ingrés realitzat. Nou saldo: {nouSaldo} €");
                }
                catch (Exception ex) { Console.WriteLine("Error de BD: " + ex.Message); }
            }
            Console.ReadLine();
        }

        static void FerRetirada()
        {
            Console.WriteLine("FER UNA RETIRADA: ");
            Console.Write("Número de compte: "); string numeroCompte = Console.ReadLine();
            Console.Write("Import: "); decimal import = Convert.ToDecimal(Console.ReadLine());
            Console.Write("Concepte: "); string concepte = Console.ReadLine();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    int compteId = ObtenirCompteId(conn, numeroCompte);
                    if (compteId == 0) { Console.WriteLine("Compte no trobat o no autoritzat."); return; }
                    decimal saldoActual = ObtenirSaldo(conn, compteId);
                    if (import > saldoActual) { Console.WriteLine("Saldo insuficient."); return; }
                    decimal nouSaldo = saldoActual - import;
                    InserirMoviment(conn, compteId, -import, concepte, nouSaldo);
                    Console.WriteLine($"Retirada realitzada. Nou saldo: {nouSaldo} €");
                }
                catch (Exception ex) { Console.WriteLine("Error de BD: " + ex.Message); }
            }
            Console.ReadLine();
        }

        static void VeureAlertes()
        {
            Console.WriteLine(" ALERTES: ");
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string sql = @"SELECT a.missatge, a.data, c.numero_compte
                                   FROM Alertes a
                                   INNER JOIN Comptes c ON a.compte_id = c.id
                                   INNER JOIN UsuarisComptes uc ON c.id = uc.compte_id
                                   WHERE uc.usuari_id = @uid";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", usuariId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                Console.WriteLine($"[{reader["data"]}] {reader["numero_compte"]}: {reader["missatge"]}");
                        }
                    }
                }
                catch (Exception ex) { Console.WriteLine("Error de BD: " + ex.Message); }
            }
            Console.ReadLine();
        }

        static int ObtenirCompteId(MySqlConnection conn, string numeroCompte)
        {
            string sql = @"SELECT c.id FROM Comptes c
                           INNER JOIN UsuarisComptes uc ON c.id = uc.compte_id
                           WHERE c.numero_compte = @num AND uc.usuari_id = @uid";
            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@num", numeroCompte);
                cmd.Parameters.AddWithValue("@uid", usuariId);
                object res = cmd.ExecuteScalar();
                return res != null ? Convert.ToInt32(res) : 0;
            }
        }

        static decimal ObtenirSaldo(MySqlConnection conn, int compteId)
        {
            string sql = "SELECT saldo FROM VistaSaldos WHERE compte_id = @cid";
            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@cid", compteId);
                object res = cmd.ExecuteScalar();
                return res != null ? Convert.ToDecimal(res) : 0;
            }
        }

        static void InserirMoviment(MySqlConnection conn, int compteId, decimal import, string concepte, decimal nouSaldo)
        {
            string sql = "INSERT INTO Moviments (compte_id, import, concepte, saldo) VALUES (@cid, @imp, @con, @sal)";
            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@cid", compteId);
                cmd.Parameters.AddWithValue("@imp", import);
                cmd.Parameters.AddWithValue("@con", concepte);
                cmd.Parameters.AddWithValue("@sal", nouSaldo);
                cmd.ExecuteNonQuery();
            }
        }

        static void RegistrarNouUsuari() { }
        static void ObrirNouCompte() { }
        static void AssignarUsuariACompte() { }
        static void LlistarUsuarisIComptes() { }
    }
}