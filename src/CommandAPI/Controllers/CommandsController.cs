using System.Collections.Generic;
using AutoMapper;
using CommandAPI.Data;
using CommandAPI.Dtos;
using CommandAPI.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
namespace CommandAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        private readonly ICommandAPIRepo _repository;
        private readonly IMapper _mapper;
        public CommandsController(ICommandAPIRepo repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpPost]
        public ActionResult <CommandReadDto> CreateCommand(CommandCreateDto commandCreateDto)
        {
            var commandModel = _mapper.Map<Command>(commandCreateDto); //4
            _repository.CreateCommand(commandModel);//5
            _repository.SaveChanges();
            var commandReadDto = _mapper.Map<CommandReadDto>(commandModel); //6
            return CreatedAtRoute(nameof(GetCommandById),new {Id = commandReadDto.Id}, commandReadDto); //7
        }
        [HttpGet]
        public ActionResult<IEnumerable<CommandReadDto>> GetAllCommands()
        {
            var commandItems = _repository.GetAllCommands();
            return Ok(_mapper.Map<IEnumerable<CommandReadDto>>(commandItems));
        }

        [HttpGet("{id}", Name = "GetCommandById")]
        public ActionResult<CommandReadDto> GetCommandById(int id)
        {
            var commandItem = _repository.GetCommandById(id);
            if (commandItem == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<CommandReadDto>(commandItem));
        }

        [HttpPut("{id}")]
        public ActionResult UpdateCommand(int id, CommandUpdateDto commandUpdateDto)
        {
            var commandModelFromRepo = _repository.GetCommandById(id); //3
            if (commandModelFromRepo == null)
            {
            return NotFound(); //4
            }
            _mapper.Map(commandUpdateDto, commandModelFromRepo); //5
            _repository.UpdateCommand(commandModelFromRepo); //6
            _repository.SaveChanges(); //7
            return NoContent(); //8
        } 
        [HttpPatch("{id}")]
        public ActionResult PartialCommandUpdate(int id, JsonPatchDocument<CommandUpdateDto> patchDoc) //2
        {
            var commandModelFromRepo = _repository.GetCommandById(id);
            if(commandModelFromRepo == null)
            {
                return NotFound();
            }
            var commandToPatch = _mapper.Map<CommandUpdateDto>(commandModelFromRepo); //4
            patchDoc.ApplyTo(commandToPatch, ModelState); //5
            if(!TryValidateModel(commandToPatch))
            {
                return ValidationProblem(ModelState); //6
            }
            _mapper.Map(commandToPatch, commandModelFromRepo); //7
            _repository.UpdateCommand(commandModelFromRepo); //8
            _repository.SaveChanges(); //9
            return NoContent(); //10
        }               
    }
}