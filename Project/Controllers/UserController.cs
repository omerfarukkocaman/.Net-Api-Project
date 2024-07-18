using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Project.Data;
using Project.Models;

namespace Project.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _distributedCache;
        private readonly IMapper _mapper;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AuthorizationService _authorizationService;

        public UsersController(ApplicationDbContext context, IDistributedCache distributedCache, IMapper mapper, IHttpClientFactory httpClientFactory, AuthorizationService authorizationService)
        {
            _context = context;
            _distributedCache = distributedCache;
            _mapper = mapper;
            _httpClientFactory = httpClientFactory;
            _authorizationService = authorizationService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(UserDto userDto, string password)
        {
            var user = _mapper.Map<User>(userDto);
            user.CreatedTime = DateTime.UtcNow;
            user.UpdatedTime = DateTime.UtcNow;
            user.Password = password;

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                };
                await _distributedCache.SetStringAsync(user.Id.ToString(), JsonSerializer.Serialize(user), cacheOptions);

                await NotifyUserCreation();

                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to create user: {ex.Message}");
            }
        }

        private async Task NotifyUserCreation()
        {
            var client = _httpClientFactory.CreateClient();
            var url = "http://localhost:7071/api/Function1";
            var content = new StringContent(JsonSerializer.Serialize(new { message = "User Created" }), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                // Hata iþleme
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var cacheKey = $"User_{id}";
            var cachedUser = await _distributedCache.GetStringAsync(cacheKey);

            UserDto userDto;

            if (!string.IsNullOrEmpty(cachedUser))
            {
                userDto = JsonSerializer.Deserialize<UserDto>(cachedUser);
            }
            else
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                userDto = _mapper.Map<UserDto>(user);
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };

                await _distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(userDto), cacheOptions);
            }

            return Ok(userDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserDto userDto)
        {
            var currentUserId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);

            // Kullanýcýnýn güncelleme iznine sahip olup olmadýðýný kontrol et
            var hasUpdatePermission = await _authorizationService.HasPermission(currentUserId, "Update");
            if (!hasUpdatePermission)
            {
                return Forbid("You do not have permission to update user information.");
            }


            if (id != userDto.Id)
            {
                return BadRequest();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.Age = userDto.Age;
            user.UpdatedTime = DateTime.UtcNow;

            _context.Users.Update(user);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            var cacheKey = $"User_{id}";
            await _distributedCache.RemoveAsync(cacheKey);

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var currentUserType = User.Claims.FirstOrDefault(c => c.Type == "UserType")?.Value;

            if (currentUserType != UserType.Admin.ToString())
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            var cacheKey = $"User_{id}";
            await _distributedCache.RemoveAsync(cacheKey);

            return NoContent();
        }
    }
}
