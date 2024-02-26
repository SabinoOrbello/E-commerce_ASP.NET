﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace BW4
{
    public partial class Dettagli : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Se non è presente l'ID del prodotto, reindirizza alla pagina principale
            if (Request.QueryString["IdProdotto"] == null)
            {
                Response.Redirect("Default.aspx");
            }

            string idString = Request.QueryString["IdProdotto"];
            int id = int.Parse(idString);

            string connectionString = ConfigurationManager.ConnectionStrings["MyDb"].ToString();
            SqlConnection conn = new SqlConnection(connectionString);

            // Carica i prodotti casuali
            CaricaProdottiCasuali(5);

            try
            {
                conn.Open();

                string query = "SELECT * FROM Prodotto WHERE IDProdotto = " + id + "AND Attivo = 1";

                SqlCommand cmd = new SqlCommand(query, conn);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    titolo.InnerText = reader.GetString(1);
                    descrizione.InnerText = reader.GetString(2);
                    prezzo.InnerText = reader.GetDecimal(3).ToString();
                    image.Src = reader.GetString(4);
                    addToCart.CommandArgument = reader.GetInt32(0).ToString();
                }
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        // Carica i prodotti casuali dal database
        protected void CaricaProdottiCasuali(int numeroProdotti)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MyDb"].ToString();
            SqlConnection conn = new SqlConnection(connectionString);

            try
            {
                conn.Open();

                string queryProdottiCasuali =
                    $"SELECT TOP {numeroProdotti} * FROM Prodotto WHERE Attivo = 1 ORDER BY NEWID()";

                SqlCommand cmdProdottiCasuali = new SqlCommand(queryProdottiCasuali, conn);

                SqlDataReader reader = cmdProdottiCasuali.ExecuteReader();

                List<Prodotto> listaProdotti = new List<Prodotto>();

                while (reader.Read())
                {
                    Prodotto prodotto = new Prodotto();
                    prodotto.Id = Convert.ToInt32(reader["IDProdotto"]);
                    prodotto.NomeProdotto = reader.GetString(1);
                    prodotto.Descrizione = reader.GetString(2);
                    prodotto.Prezzo = reader.GetDecimal(3);
                    prodotto.Immagine = reader.GetString(4);

                    listaProdotti.Add(prodotto);
                }

                Repeater2.DataSource = listaProdotti;
                Repeater2.DataBind();
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        // Aggiunge il prodotto al carrello e reindirizza alla stessa pagina
        protected void addToCart_Click(object sender, EventArgs e)
        {
            string idString = ((Button)sender).CommandArgument;
            int id = int.Parse(idString);

            // Se la quantità è vuota, imposta la quantità a 1
            int quantity = !string.IsNullOrEmpty(quantityInput.Value)
                ? int.Parse(quantityInput.Value)
                : 1;

            string connectionString = ConfigurationManager.ConnectionStrings["MyDb"].ToString();
            SqlConnection conn = new SqlConnection(connectionString);

            try
            {
                conn.Open();

                string query = "SELECT * FROM Prodotto WHERE IDProdotto = " + id;

                SqlCommand cmd = new SqlCommand(query, conn);

                SqlDataReader reader = cmd.ExecuteReader();
                // se il carrello non esiste, crea un nuovo carrello
                if (Session["cart"] == null)
                {
                    Session["cart"] = new List<Prodotto>();
                }

                if (reader.Read())
                {
                    // Aggiunge il prodotto al carrello
                    List<Prodotto> cart = (List<Prodotto>)Session["cart"];
                    Prodotto prodotto = new Prodotto();
                    prodotto.Id = Convert.ToInt32(reader["IDProdotto"]);
                    prodotto.NomeProdotto = reader.GetString(1);
                    prodotto.Descrizione = reader.GetString(2);
                    prodotto.Prezzo = reader.GetDecimal(3);
                    prodotto.Immagine = reader.GetString(4);

                    for (int i = 0; i < quantity; i++)
                    {
                        cart.Add(prodotto);
                    }
                    // Aggiorna il carrello nella sessione e reindirizza alla stessa pagina
                    Session["cart"] = cart;
                    Response.Redirect(Request.RawUrl);
                }
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
