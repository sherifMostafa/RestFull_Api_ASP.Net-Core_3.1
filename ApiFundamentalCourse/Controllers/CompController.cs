using ApiFundamentalCourse.Models;
using AutoMapper;
using CoreCodeCamp.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFundamentalCourse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("1.1")]
    public class CompController : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _link;

        public CompController( ICampRepository repository , IMapper mapper, LinkGenerator Link)
        {
            _repository = repository;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<CampModel[]>> Get(bool includeTalks = false)
        {
            try
            {
                var result = await _repository.GetAllCampsAsync(includeTalks);
                return _mapper.Map<CampModel[]>(result);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failier");
            }
        }
        [HttpGet("{moniker}")]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult<CampModel>> Get(string moniker, bool includeTalks = false)
        {
            try
            {
                var result = await _repository.GetCampAsync(moniker);
                if (result == null)
                    return NotFound();
                else
                {
                    return _mapper.Map<CampModel>(result);
                }
            } 
            catch(Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failier");
            }
          }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime date, bool includeTalks = false)
        {
            try
            {
                var result = await _repository.GetAllCampsByEventDate(date , includeTalks);
                if (!result.Any())
                    return NotFound();
                else
                {
                    return _mapper.Map<CampModel[]>(result);
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failier");
            }
        }

        public async Task<ActionResult<Camp>> Post(CampModel model )
        {
            try
            {
                var camp = _mapper.Map<Camp>(model);
                _repository.Add(camp);
                if( await _repository.SaveChangesAsync())
                {
                    return Created(@"/api/Get/",_mapper.Map<CampModel>(camp));
                }
               
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failier");
            }
            return BadRequest();
        } 
        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> Put(string moniker , CampModel model)
        {
            try
            {
                var oldmodel = await _repository.GetCampAsync(moniker);
                if(oldmodel == null)
                {
                    return NotFound($"Could Not Found  Camp With Monikir of {moniker}");
                }

                _mapper.Map(model, oldmodel);
                if(await _repository.SaveChangesAsync())
                {
                    return _mapper.Map<CampModel>(oldmodel);
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failier");
            }
            return BadRequest();
        }
        [HttpDelete("{moniker}")]
        public async Task<ActionResult<CampModel>> Delete(string moniker)
        {
            var camp = await _repository.GetCampAsync(moniker);
            if (camp == null)
                return NotFound();
            _repository.Delete(camp);
            if (await _repository.SaveChangesAsync())
                return Ok();
            return BadRequest();
        }

    }
}

