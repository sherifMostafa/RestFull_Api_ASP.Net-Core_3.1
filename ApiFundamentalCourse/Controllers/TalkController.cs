using AutoMapper;
using CoreCodeCamp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiFundamentalCourse.Models;

namespace ApiFundamentalCourse.Controllers
{
    [ApiController]
    [Route("/api/Camps/{moniker}/talks")]
    public class TalkController : ControllerBase
    {
        private readonly ICampRepository _repository; 
        private readonly IMapper _mapper; 
        private readonly LinkGenerator _link; 
        public TalkController(ICampRepository repository , IMapper mapper, LinkGenerator Link)
        {
            _repository = repository;
            _mapper = mapper;
            _link = Link;
        }

        [HttpGet]
        public async Task<ActionResult<Talk[]>> Get(string moniker)
        {
            try
            {
                var talks = await _repository.GetTalksByMonikerAsync(moniker, true);
                if (talks == null) return BadRequest("Could not find talks");
                return _mapper.Map<Talk[]>(talks);

            }
            catch(Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError , "faild database");
            }
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Talk>> get(string moniker , int id)
        {
            try
            {
                var talk = await _repository.GetTalkByMonikerAsync(moniker,id,true);
                if (talk == null) return BadRequest("Could not find talk");
                return _mapper.Map<Talk>(talk);
            } catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "failier Database");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Talk>> Post(string moniker, TalkModel model)
        {
            try
            {
                var talk = _mapper.Map<Talk>(model);
                var camp = await _repository.GetCampAsync(moniker);
                if (camp == null) return BadRequest("Camp Not Exist");
                talk.Camp = camp;

                if (model.Speaker == null) return BadRequest("Speaker Id Is Required");
                var speaker = await _repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                if (speaker == null) return BadRequest("Speaker Could Not be Found");
                talk.Speaker = speaker;
                _repository.Add(talk);
                if(await _repository.SaveChangesAsync())
                {
                    var link = _link.GetPathByAction(HttpContext, "Get",values: new {moniker , id=talk.TalkId });
                    return Created(link, _mapper.Map<TalkModel>(talk));
                }
                else
                {
                    return BadRequest("Failed To Save New Talk");
                }               
            }
            catch
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "failier Database");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TalkModel>> Put(string moniker, int id , TalkModel model)
        {
            try
            {
                var talk = await _repository.GetTalkByMonikerAsync(moniker, id , true);
                if (talk == null) return BadRequest("Talk Not Found ");
                _mapper.Map(model, talk);

                if(model.Speaker != null)
                {
                    var speaker = await _repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                    if (speaker != null) talk.Speaker = speaker;
                }
                if (await _repository.SaveChangesAsync())
                {
                    return _mapper.Map<TalkModel>(talk);
                }
                
            }
            catch
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "failier Database");
            }
            return BadRequest();
        }
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<Talk>> Delete(string moniker, int id)
        {
            try
            {
                var talk = await _repository.GetTalkByMonikerAsync(moniker, id);
                if (talk == null) return BadRequest("Could not find talk ");
                _repository.Delete(talk);
                if(await _repository.SaveChangesAsync())
                {
                    return Ok();
                } 
            }
            catch
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "failier Database");
            }
            return BadRequest();
        }

    }
}
