using API.RequestHelpers;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IGenericRepository<Product> productRepo) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts([FromQuery]ProductSpecParams productSpecParams)
    {
        var spec = new ProductSpecification(productSpecParams);

        return await CreatePagedResult(productRepo, spec,productSpecParams.PageIndex, productSpecParams.PageSize);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await productRepo.GetByIdAsync(id);

        if (product == null) return NotFound();

        return product;
    }

    [HttpPost]
    public async Task<ActionResult<Product>> AddProduct(Product product)
    {
        productRepo.Add(product);

        if (await productRepo.SaveAllAsync())
        {
            return CreatedAtAction("GetProduct", new {id = product.Id}, product);
        }

        return BadRequest("An error occured while add a product.");
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateProduct(int id, Product product)
    {
        if (product.Id != id || !ProductExists(id))
            return BadRequest("Product does not exist to update.");

        productRepo.Update(product);
        
        if (await productRepo.SaveAllAsync())
        {
            return NoContent();
        }

        return BadRequest("Error while updating the product.");
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        var product = await productRepo.GetByIdAsync(id);

        if (product == null) return NotFound();

        productRepo.Remove(product);

        if (await productRepo.SaveAllAsync())
        {
            return NoContent();
        }

        return BadRequest("Error while deleting the product.");
    }

    [HttpGet("brands")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetBrands()
    {
        var spec = new BrandListSpecification();

        return Ok(await productRepo.ListAsync(spec));
    }

    [HttpGet("types")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetTypes()
    {
        var spec = new TypeListSpecification();

        return Ok(await productRepo.ListAsync(spec));
    }

    private bool ProductExists(int id)
    {
        return productRepo.Exists(id);
    }
}
