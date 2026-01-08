using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stock.Context;
using Stock.Models;
using Stock.Models.ViewModels;
using System;

namespace Stock.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly DataContext db;
        public LocationController(DataContext db) => this.db = db;

        [HttpGet("Get")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<Location>>> GetAll()
        {
            try
            {
                var items = await db.Locations.Where(w => w.IsActive).OrderBy(o => o.Name).ToListAsync();

                if (items.Count == 0) return Ok(new List<Location>());
                return Ok(items);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        [HttpGet("GetById/{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<Location>>> GetDataById(int id)
        {
            try
            {
                var item = await db.Locations.Where(w => w.Id == id).SingleOrDefaultAsync();

                if (item == null) return Ok(new Location());
                return Ok(item);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        [HttpPost("Post")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<Location>> Save(Location item)
        {
            try
            {
                var location = new Location()
                {
                    Name = item.Name,
                    Descprition = item.Descprition,
                    IsActive = true,
                    InputBy = item.InputBy,
                };
                db.Locations.Add(location);
                await db.SaveChangesAsync();

                return Ok(location);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        [HttpPut("Put")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<Location>> Update(Location item)
        {
            try
            {
                var location = await db.Locations.FindAsync(item.Id);
                if (location == null) return BadRequest();
                location.Name = item.Name;
                location.Descprition = item.Descprition;
                location.ModifyDate = DateTime.Now;
                location.ModifyBy = item.InputBy;

                await db.SaveChangesAsync();
                return Ok(location);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        [HttpPost("Delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<Location>> Remove(ActionVM item)
        {
            try
            {
                var location = await db.Locations.FindAsync(item.Id);
                if (location == null) return BadRequest();
                location.IsActive = false;
                location.ModifyDate = DateTime.Now;
                location.ModifyBy = item.InputBy;

                await db.SaveChangesAsync();
                return Ok(location);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}
