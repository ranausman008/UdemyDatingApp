using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUserRepository userRepository;
    private readonly IMapper mapper;
    private readonly IPhotoService photoService;

    public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
    {
        this.userRepository = userRepository;
        this.mapper = mapper;
        this.photoService = photoService;
    }

    
    [HttpGet]
    public async Task<ActionResult<PagedList<MemberDTO>>> GetUsers([FromQuery]UserParams userParams)
    {
        var currentUser = await userRepository.GetUserByUserNameAsync(User.GetUsername());

        userParams.CurrentUsername= currentUser.UserName;

        if(string.IsNullOrEmpty(userParams.Gender))
        {
            userParams.Gender = currentUser.Gender == "male" ? "female" : "male";
        }

        var users = await userRepository.GetMembersAsync(userParams);

        Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));

        return Ok(users);
     
    }


    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDTO>> GetUser(string username)
    {
        return await userRepository.GetMemberAsync(username);

        
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
    {
        var user = await userRepository.GetUserByUserNameAsync(User.GetUsername());

        if(user == null) { return NotFound(); }

        mapper.Map(memberUpdateDTO, user);

        if (await userRepository.SaveAllAsync()) return NoContent();

        return BadRequest("Failed To Update User");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
    {
        var user = await userRepository.GetUserByUserNameAsync(User.GetUsername());

        if(user == null) { return NotFound();}

        var result = await photoService.AddPhotoAsync(file);

        if(result.Error != null) return BadRequest(result.Error.Message);

        var photo = new Photo
        {
            URL = result.SecureUrl.AbsoluteUri,
            PublicID = result.PublicId
        };

        if(user.Photos.Count == 0) photo.isMain= true;

        user.Photos.Add(photo);

        if(await userRepository.SaveAllAsync())
        {
            return CreatedAtAction(nameof(GetUser), new {username = user.UserName}, mapper.Map<PhotoDTO>(photo));
        }

        return BadRequest("Problem Adding Photo");
    }

    [HttpPut("set-main-photo/{photoid}")]
    public async Task<ActionResult> SetMainPhoto(int photoid)
    {
        var user = await userRepository.GetUserByUserNameAsync(User.GetUsername());

        if(user == null) { return NotFound();}

        var photo = user.Photos.FirstOrDefault(x => x.ID == photoid);

        if(photo == null) return NotFound();

        if (photo.isMain) return BadRequest("This is already Main Photo");

        var currentMain = user.Photos.FirstOrDefault(x => x.isMain);

        if(currentMain != null) currentMain.isMain = false;

        photo.isMain = true;

        if (await userRepository.SaveAllAsync()) return NoContent();

        return BadRequest("Problem Setting Main Photo");
    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await userRepository.GetUserByUserNameAsync(User.GetUsername());

        var photo = user.Photos.FirstOrDefault(x => x.ID == photoId);

        if (photo == null) return NotFound();

        if (photo.isMain) return BadRequest("You cannot delete your main photo");

        if (photo.PublicID != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicID);
            if (result.Error != null) return BadRequest(result.Error.Message);
        }

        user.Photos.Remove(photo);

        if (await userRepository.SaveAllAsync()) return Ok();

        return BadRequest("Problem deleting photo");
    }

}