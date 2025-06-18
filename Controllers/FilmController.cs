using System;
using System.Collections.Generic;
using System.Web.Mvc;
using DVD_Rental.Models;
using DVD_Rental.Helpers;

namespace DVD_Rental.Controllers
{
    [Filters.StaffAuthorizeAttribute(Permission = "Manage Inventory")]
    public class FilmController : Controller
    {
        public ActionResult Index(string search)
        {
            var films = new List<FilmModel>();

            using (var db = new SqlHelper())
            {
                string sql = @"
    SELECT f.film_id, f.title, f.description, f.release_year,
           l.name AS language, f.length, f.rating
    FROM film f
    JOIN language l ON f.language_id = l.language_id
    WHERE (@search IS NULL OR f.title ILIKE @search)
    ORDER BY f.title";

                films = db.ExecuteQuery(
                    sql,
                    p =>
                    {
                        if (string.IsNullOrEmpty(search))
                        {
                            p.Add("search", NpgsqlTypes.NpgsqlDbType.Text).Value = DBNull.Value;
                        }
                        else
                        {
                            p.Add("search", NpgsqlTypes.NpgsqlDbType.Text).Value = search + "%";
                        }
                    },
                    reader => new FilmModel
                    {
                        FilmId = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        ReleaseYear = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                        Language = reader.GetString(4),
                        Length = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                        Rating = reader.IsDBNull(6) ? "" : reader.GetString(6)
                    }
                );

            }

            ViewBag.Search = search;
            return View(films);
        }

        public ActionResult Detail(int id)
        {
            var model = new FilmDetailModel();

            using (var db = new SqlHelper())
            {
                // Film basic info
                var filmInfo = db.ExecuteSingle(
                    @"SELECT f.film_id, f.title, f.description, f.release_year, l.name AS language,
                             f.length, f.rating, f.rental_rate, f.rental_duration, f.replacement_cost
                      FROM film f
                      JOIN language l ON f.language_id = l.language_id
                      WHERE f.film_id = @id",
                    p => p.AddWithValue("id", id),
                    reader => new
                    {
                        FilmId = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        ReleaseYear = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                        Language = reader.GetString(4),
                        Length = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                        Rating = reader.IsDBNull(6) ? "" : reader.GetString(6),
                        RentalRate = reader.GetDecimal(7),
                        RentalDuration = reader.GetInt32(8),
                        ReplacementCost = reader.GetDecimal(9)
                    }
                );

                if (filmInfo != null)
                {
                    model.FilmId = filmInfo.FilmId;
                    model.Title = filmInfo.Title;
                    model.Description = filmInfo.Description;
                    model.ReleaseYear = filmInfo.ReleaseYear;
                    model.Language = filmInfo.Language;
                    model.Length = filmInfo.Length;
                    model.Rating = filmInfo.Rating;
                    model.RentalRate = filmInfo.RentalRate;
                    model.RentalDuration = filmInfo.RentalDuration;
                    model.ReplacementCost = filmInfo.ReplacementCost;
                }

                // Actors
                var actorNames = db.ExecuteQuery(
                    @"SELECT a.first_name, a.last_name
                      FROM actor a
                      JOIN film_actor fa ON a.actor_id = fa.actor_id
                      WHERE fa.film_id = @id",
                    p => p.AddWithValue("id", id),
                    r => $"{r.GetString(0)} {r.GetString(1)}"
                );
                model.Actors.AddRange(actorNames);

                // Categories
                var categoryNames = db.ExecuteQuery(
                    @"SELECT c.name
                      FROM category c
                      JOIN film_category fc ON c.category_id = fc.category_id
                      WHERE fc.film_id = @id",
                    p => p.AddWithValue("id", id),
                    r => r.GetString(0)
                );
                model.Categories.AddRange(categoryNames);
            }

            return View(model);
        }
    }
}
