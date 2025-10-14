using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PI_API.dto;
using PI_API.models;
using PI_API.services;

namespace PI_API.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        UserService _userService;
        public UserController(UserService userService)
        {
            _userService =  userService;
        }
        
        [HttpPost]
        public async Task<ActionResult<List<User>>> Create([FromBody] UserDTO userDto)
        {
            User newUser = new User();
            
            newUser.Email = userDto.Email;
            newUser.CpfCnpj = userDto.CpfCnpj;
            newUser.FullName = userDto.FullName;
            newUser.Name = userDto.Name;
            
            newUser.CreatedAt = DateTime.Now;
            newUser.UpdatedAt = DateTime.Now;
            newUser.Active = false;
            
            
            await _userService.CreateAsync(newUser);
            return Ok();
        }


        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAll()
        {
            List<User> list = await _userService.GetAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> Get(string id)
        {
            return Ok(await _userService.GetByIdAsync(id));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            await _userService.DeleteAsync(id);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(string id, [FromBody] UserDTO userDto)
        {
            User updatedUser = await _userService.GetByIdAsync(id);
            if (updatedUser == null)
            {
                return BadRequest("User not found");
            }
            updatedUser.FullName = userDto.FullName;
            updatedUser.Email = userDto.Email;
            updatedUser.CpfCnpj = userDto.CpfCnpj;
            updatedUser.Name = userDto.Name;
            updatedUser.UpdatedAt = DateTime.Now;
            updatedUser.Active = true;
            
            _userService.UpdateAsync(id, updatedUser);
            return Ok();
            
            
        }

        
        
        



        
    }
}
