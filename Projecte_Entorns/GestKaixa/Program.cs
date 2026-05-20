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
            Console.Clear();
            FuncioPrincipal();
        }

        static void FuncioPrincipal()
        {
            bool esAdmin;
            UsuariIValidar(out esAdmin);
            if (esAdmin)
                GestionarMenuAdmin();
            else if (usuariId != 0)
                GestionarMenuClient();
        }

        static void UsuariIValidar(out bool esAdmin)
        {
            Console.WriteLine("BENVINGUT A GESTKAIXA");
            Console.Write("Usuari: ");
            string usuari = Console.ReadLine();
            Console.Write("Contrasenya: ");
            string password = LlegirPasswordOcult();

            if (usuari == "cashbox_app" && password == "app123")
                esAdmin = true;
            else if (ValidarClientABaseDeDades(usuari, password))
                esAdmin = false;
            else
            {
                Console.WriteLine("Accés denegat. Credencials incorrectes.");
                esAdmin = false;
            }
        }

        static string LlegirPasswordOcult()
        {
            string password = "";
            ConsoleKeyInfo tecla;
            do
            {
                tecla = Console.ReadKey(true);
                if (tecla.Key != ConsoleKey.Enter && tecla.Key != ConsoleKey.Backspace)
                {
                    password += tecla.KeyChar;
                    Console.Write("*");
                }
                else if (tecla.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
            } while (tecla.Key != ConsoleKey.Enter);
            Console.WriteLine();
            return password;
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
                catch (Exception ex) { Console.WriteLine("Error de BD: " + ex.Message); }
            }
            return loginCorrecte;
        }

        static int MostrarMenuAdmin()
        {
            int opcio;
            Console.Clear();
            Console.WriteLine("SISTEMA GESTKAIXA - MÒDUL ADMINISTRADOR");
            Console.WriteLine("1. Registrar nous usuaris");
            Console.WriteLine("2. Obrir nous comptes.");
            Console.WriteLine("3. Assignar usuaris a comptes.");
            Console.WriteLine("4. Llistar tots els usuaris i els comptes associats.");
            Console.WriteLine("5. Llistar tots els usuaris.");
            Console.WriteLine("6. Sortir");
            Console.Write("Opció: ");
            opcio = Convert.ToInt32(Console.ReadLine());
            return opcio;
        }

        static int MostrarMenuClient()
        {
            int opcio;
            Console.Clear();
            Console.WriteLine("SISTEMA GESTKAIXA - MÒDUL CLIENT");
            Console.WriteLine("1. Veure els seus comptes.");
            Console.WriteLine("2. Consultar el saldo.");
            Console.WriteLine("3. Veure moviments.");
            Console.WriteLine("4. Fer un ingrés.");
            Console.WriteLine("5. Fer una retirada.");
            Console.WriteLine("6. Veure alertes");
            Console.WriteLine("7. Sortir");
            Console.Write("Opció: ");
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
                    case 5: LlistarTotsElsUsuaris(); break;
                    case 6: Console.WriteLine("Sortint..."); break;
                    default: Console.WriteLine("Opció no vàlida."); break;
                }
            } while (opcio != 6);
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
            Console.Clear();
            Console.WriteLine(" ===COMPTES ASSOCIATS=== ");
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
                            while (reader.Read())
                                Console.WriteLine($"Número: {reader["numero_compte"]} | Estat: {reader["estat"]} | Data: {reader["data_creacio"]}");
                    }
                }
                catch (Exception ex) { Console.WriteLine("Error de BD: " + ex.Message); }
            }
            Console.WriteLine("\nPrem Enter per continuar...");
            Console.ReadLine();
        }

        static void ConsultarSaldo()
        {
            Console.Clear();
            Console.WriteLine(" ===CONSULTA DE SALDO=== ");
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
                            while (reader.Read())
                                Console.WriteLine($"Compte: {reader["numero_compte"]} | Saldo: {reader["saldo"]} €");
                    }
                }
                catch (Exception ex) { Console.WriteLine("Error de BD: " + ex.Message); }
            }
            Console.WriteLine("\nPrem Enter per continuar...");
            Console.ReadLine();
        }

        static void VeureMoviments()
        {
            Console.Clear();
            Console.WriteLine(" ===MOVIMENTS=== ");
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
                            while (reader.Read())
                                Console.WriteLine($"[{reader["data"]}] {reader["numero_compte"]} | {reader["concepte"]} | Import: {reader["import"]} € | Saldo: {reader["saldo"]} €");
                    }
                }
                catch (Exception ex) { Console.WriteLine("Error de BD: " + ex.Message); }
            }
            Console.WriteLine("\nPrem Enter per continuar...");
            Console.ReadLine();
        }

        static void FerIngres()
        {
            Console.Clear();
            Console.WriteLine(" ===FER UN INGRÉS=== ");
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
            Console.WriteLine("\nPrem Enter per continuar...");
            Console.ReadLine();
        }

        static void FerRetirada()
        {
            Console.Clear();
            Console.WriteLine(" ===FER UNA RETIRADA=== ");
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
            Console.WriteLine("\nPrem Enter per continuar...");
            Console.ReadLine();
        }

        static void VeureAlertes()
        {
            Console.Clear();
            Console.WriteLine(" ===ALERTES=== ");
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
                            while (reader.Read())
                                Console.WriteLine($"[{reader["data"]}] {reader["numero_compte"]}: {reader["missatge"]}");
                    }
                }
                catch (Exception ex) { Console.WriteLine("Error de BD: " + ex.Message); }
            }
            Console.WriteLine("\nPrem Enter per continuar...");
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

        static void RegistrarNouUsuari()
        {
            Console.Clear();
            Console.WriteLine(" ===REGISTRAR NOU USUARI=== ");
            Console.Write("DNI: "); string dni = Console.ReadLine();
            Console.Write("Nom: "); string nom = Console.ReadLine();
            Console.Write("Cognom: "); string cognom = Console.ReadLine();
            Console.Write("Adreça: "); string adreca = Console.ReadLine();
            Console.Write("Telèfon: "); string telefon = Console.ReadLine();
            Console.Write("Username: "); string username = Console.ReadLine();
            Console.Write("Password: "); string password = LlegirPasswordOcult();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "INSERT INTO Usuaris (dni, nom, cognom, adreca, telefon, username, password) VALUES (@dni, @nom, @cog, @adr, @tel, @usr, @pass)";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@dni", dni);
                        cmd.Parameters.AddWithValue("@nom", nom);
                        cmd.Parameters.AddWithValue("@cog", cognom);
                        cmd.Parameters.AddWithValue("@adr", adreca);
                        cmd.Parameters.AddWithValue("@tel", telefon);
                        cmd.Parameters.AddWithValue("@usr", username);
                        cmd.Parameters.AddWithValue("@pass", password);
                        cmd.ExecuteNonQuery();
                    }
                    Console.WriteLine("Usuari registrat correctament.");
                }
                catch (Exception ex) { Console.WriteLine("Error de BD: " + ex.Message); }
            }
            Console.WriteLine("\nPrem Enter per continuar...");
            Console.ReadLine();
        }

        static void ObrirNouCompte()
        {
            Console.Clear();
            Console.WriteLine(" ===OBRIR NOU COMPTE=== ");
            Console.Write("Número de compte: "); string numeroCompte = Console.ReadLine();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "INSERT INTO Comptes (numero_compte) VALUES (@num)";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@num", numeroCompte);
                        cmd.ExecuteNonQuery();
                    }
                    Console.WriteLine("Compte obert correctament.");
                }
                catch (Exception ex) { Console.WriteLine("Error de BD: " + ex.Message); }
            }
            Console.WriteLine("\nPrem Enter per continuar...");
            Console.ReadLine();
        }

        static void AssignarUsuariACompte()
        {
            Console.Clear();
            Console.WriteLine(" ===ASSIGNAR USUARI A COMPTE=== ");
            Console.Write("Username de l'usuari: "); string username = Console.ReadLine();
            Console.Write("Número de compte: "); string numeroCompte = Console.ReadLine();
            Console.Write("Rol (TITULAR/AUTORITZAT): "); string rol = Console.ReadLine();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string sqlUsuari = "SELECT id FROM Usuaris WHERE username = @usr";
                    int uid = 0;
                    using (MySqlCommand cmd = new MySqlCommand(sqlUsuari, conn))
                    {
                        cmd.Parameters.AddWithValue("@usr", username);
                        object res = cmd.ExecuteScalar();
                        if (res != null) uid = Convert.ToInt32(res);
                    }
                    if (uid == 0) { Console.WriteLine("Usuari no trobat."); Console.ReadLine(); return; }

                    string sqlCompte = "SELECT id FROM Comptes WHERE numero_compte = @num";
                    int cid = 0;
                    using (MySqlCommand cmd = new MySqlCommand(sqlCompte, conn))
                    {
                        cmd.Parameters.AddWithValue("@num", numeroCompte);
                        object res = cmd.ExecuteScalar();
                        if (res != null) cid = Convert.ToInt32(res);
                    }
                    if (cid == 0) { Console.WriteLine("Compte no trobat."); Console.ReadLine(); return; }

                    string sqlAssignar = "INSERT INTO UsuarisComptes (usuari_id, compte_id, rol) VALUES (@uid, @cid, @rol)";
                    using (MySqlCommand cmd = new MySqlCommand(sqlAssignar, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", uid);
                        cmd.Parameters.AddWithValue("@cid", cid);
                        cmd.Parameters.AddWithValue("@rol", rol);
                        cmd.ExecuteNonQuery();
                    }
                    Console.WriteLine("Usuari assignat al compte correctament.");
                }
                catch (Exception ex) { Console.WriteLine("Error de BD: " + ex.Message); }
            }
            Console.WriteLine("\nPrem Enter per continuar...");
            Console.ReadLine();
        }

        static void LlistarUsuarisIComptes()
        {
            Console.Clear();
            Console.WriteLine(" ===USUARIS I COMPTES=== ");
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string sql = @"SELECT u.username, u.nom, u.cognom, c.numero_compte, uc.rol
                                   FROM Usuaris u
                                   INNER JOIN UsuarisComptes uc ON u.id = uc.usuari_id
                                   INNER JOIN Comptes c ON uc.compte_id = c.id
                                   ORDER BY u.username";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                        while (reader.Read())
                            Console.WriteLine($"{reader["username"]} | {reader["nom"]} {reader["cognom"]} | Compte: {reader["numero_compte"]} | Rol: {reader["rol"]}");
                }
                catch (Exception ex) { Console.WriteLine("Error de BD: " + ex.Message); }
            }
            Console.WriteLine("\nPrem Enter per continuar...");
            Console.ReadLine();
        }

        static void LlistarTotsElsUsuaris()
        {
            Console.Clear();
            Console.WriteLine(" ===LLISTAT DE TOTS ELS USUARIS=== ");
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT id, dni, nom, cognom, username, telefon FROM Usuaris ORDER BY cognom";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                        while (reader.Read())
                            Console.WriteLine($"[{reader["id"]}] {reader["nom"]} {reader["cognom"]} | DNI: {reader["dni"]} | Username: {reader["username"]} | Tel: {reader["telefon"]}");
                }
                catch (Exception ex) { Console.WriteLine("Error de BD: " + ex.Message); }
            }
            Console.WriteLine("\nPrem Enter per continuar...");
            Console.ReadLine();
        }
    }
}