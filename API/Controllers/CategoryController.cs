﻿using API.DTOs;
using API.Models;
using API.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoryController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public CategoryController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    [HttpGet("tree")]
    [ProducesResponseType(typeof(IEnumerable<Category>), StatusCodes.Status200OK)]
    public IActionResult GetCategoryTree()
    {
        var roots = _uow.CategoryRepository.GetRootCategories();
        return Ok(roots);
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(Category), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            ParentCategoryId = dto.ParentCategoryId
        };

        _uow.CategoryRepository.Add(category);
        _uow.Save();

        return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetCategoryById(int id)
    {
        var cat = _uow.CategoryRepository.GetById(id);
        return cat == null ? NotFound() : Ok(cat);
    }

    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Rename(int id, [FromBody] string newName)
    {
        var category = _uow.CategoryRepository.GetById(id);
        if (category == null) return NotFound();

        category.ModifyCategory(newName);
        _uow.CategoryRepository.Update(category);
        _uow.Save();

        return Ok(category);
    }

    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Delete(int id)
    {
        var category = _uow.CategoryRepository.GetById(id);
        if (category == null) return NotFound();

        if (!_uow.CategoryRepository.IsCategoryEmpty(id))
            return BadRequest("Cannot delete non-empty category.");

        _uow.CategoryRepository.Delete(id);
        _uow.Save();

        return NoContent();
    }

    [Authorize]
    [HttpPost("rearrange")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Rearrange([FromBody] CategoryReorderDto dto)
    {
        try
        {
            _uow.CategoryRepository.Rearrange(dto.CategoryId, dto.NewParentId, dto.NewPosition);
            _uow.Save();
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpPost("{id}/display")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult SetDisplayPreference(int id, [FromBody] bool isExpanded)
    {
        var username = User.Identity?.Name;
        var user = _uow.UserRepository.GetByUsername(username!);
        if (user == null) return Unauthorized();

        _uow.CategoryRepository.SetDisplayPreference(id, user.Id, isExpanded);
        _uow.Save();

        return Ok("Preference saved.");
    }

}
