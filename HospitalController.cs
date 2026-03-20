using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.Marshalling;
using System.Security.Cryptography.X509Certificates;
namespace HastaneOtomasyonu.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HastaneController : ControllerBase
    {
        public string _db = $"Data Source = {Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\HastaneDb.db";



        public HastaneController()
        {
            using (SqliteConnection conn = new SqliteConnection(_db))
            {
                conn.Open();

                string sql = @"
            CREATE TABLE IF NOT EXISTS Randevular (
                Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                Tc TEXT, 
                DoktorId INTEGER, 
                Tarih TEXT, 
                Saat TEXT
            );";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }

            }


        }
        [HttpGet("doktorlar-listesi")]
        public IActionResult GetDoktorlar()
        {
            var liste = new List<object>();
            using (SqliteConnection conn = new SqliteConnection(_db))
            {
                conn.Open();
                string sqlkomut = "Select Id, AdSoyad, Brans From Doktorlar";
                using (SqliteCommand cmd = new SqliteCommand(sqlkomut, conn))
                using (SqliteDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        liste.Add(new { id = rd["Id"], adSoyad = rd["AdSoyad"], brans = rd["Brans"] });
                    }
                }
            }
            return Ok(liste);
        }
        [HttpGet("dolu-saatler")]
        public IActionResult GetDoluSaatler(int doktorId, string tarih)
        {
            var doluSaatler = new List<string>();
            using (SqliteConnection conn = new SqliteConnection(_db))
            {
                conn.Open();
                string sql = "Select Saat From Randevular Where DoktorId = @did and Tarih = @t";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@did", doktorId);
                    cmd.Parameters.AddWithValue("@t", tarih);
                    using (SqliteDataReader rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            doluSaatler.Add(rd["Saat"].ToString());
                        }
                    }
                }
            }
            return Ok(doluSaatler);
        }

        [HttpGet("tum-randevular")]
        public IActionResult GetRandevular()
        {
            var liste = new List<object>();
            using (SqliteConnection conn = new SqliteConnection(_db))
            {
                conn.Open();
                string sqlGetir = "Select h.Id, h.Tc, d.AdSoyad, h.Tarih, h.Saat From Randevular h JOIN Doktorlar d ON h.DoktorId = d.Id";
                using (SqliteCommand cmd = new SqliteCommand(sqlGetir, conn))
                using (SqliteDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        liste.Add(new { id = rdr[0], tc = rdr[1], doktor = rdr[2], tarih = rdr[3], saat = rdr[4] });
                    }
                }
            }
            return Ok(liste);
        }
        [HttpDelete("Randevu-sil")]
        public IActionResult RandecuSil(int Id)
        {
            using (SqliteConnection conn = new SqliteConnection(_db))
            {
                conn.Open();

                string sqlkomut = "Select Count(*) From Randevular Where Id = @id";
                using (SqliteCommand cmd2 = new SqliteCommand(sqlkomut, conn))
                {
                    cmd2.Parameters.AddWithValue("@id", Id);
                    int Rndvarmı = Convert.ToInt32(cmd2.ExecuteScalar());

                    if(Rndvarmı == 0)
                    {
                        return BadRequest("Eşleşemeyen .ID");
                    }
                }

                string sqlsil = "Delete From Randevular Where Id = @id";
                using (SqliteCommand cmd = new SqliteCommand(sqlsil, conn))
                {
                    cmd.Parameters.AddWithValue("@id", Id);
                    cmd.ExecuteNonQuery();
                }
                return Ok(new { mesaj = "Randevu iptal edilmiştir" });
            }
        }
        [HttpPost("randevu-al")]

        public IActionResult RandevuAl(string tc, int doktorId, string tarih, string saat)
        {
            try
            {
                using (SqliteConnection conn = new SqliteConnection(_db))
                {
                    conn.Open();

                    string kontrolsql = "Select Count(*) From Randevular Where doktorId = @did and tarih = @t and saat = @s";
                    using (SqliteCommand cmd = new SqliteCommand(kontrolsql, conn))
                    {
                        cmd.Parameters.AddWithValue("@did", doktorId);
                        cmd.Parameters.AddWithValue("@t", tarih);
                        cmd.Parameters.AddWithValue("@s", saat);

                        int dolumu = Convert.ToInt32(cmd.ExecuteScalar());
                        if (dolumu > 0)
                        {
                            return BadRequest("Bu randevu zaten alınmış.");
                        }
                    }
                    string sqlekle = "Insert Into Randevular (tc, doktorId, tarih, saat) Values (@tc, @did, @t, @s)";
                    using (SqliteCommand cmd = new SqliteCommand(sqlekle, conn))
                    {
                        cmd.Parameters.AddWithValue("@tc", tc);
                        cmd.Parameters.AddWithValue("@did", doktorId);
                        cmd.Parameters.AddWithValue("@t", tarih);
                        cmd.Parameters.AddWithValue("@s", saat);

                        cmd.ExecuteNonQuery();

                    }
                }
                return Ok(new { mesaj = "Randevunuz başarıyla oluşturuldu." });
            }

            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }

        }
    }
}
