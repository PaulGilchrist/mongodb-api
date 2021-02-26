//using MongoDbApi.Models;
//using MongoDbApi.Services;
//using Microsoft.AspNetCore.Mvc;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace MongoDbApi.Controllers {
//    [ApiController]
//    [Route("api/[controller]")]
//    public class ContactsController: ControllerBase {

//        private readonly ContactService _contactService;


//        public ContactsController(ContactService contactService) {
//            _contactService = contactService;
//        }

//        [HttpGet]
//        public async Task<IActionResult> Get() {
//            return Ok(await _contactService.Get());
//        }

//        [HttpGet("{id:length(24)}",Name = "GetContact")]
//        public async Task<IActionResult> Get(string id) {
//            var contact = await _contactService.Get(id);
//            if(contact == null) {
//                return NotFound();
//            }
//            return Ok(contact);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Create(Contact inContact) {
//            var contact = await _contactService.Create(inContact);
//            return Created("", contact);
//        }

//        [HttpPut("{id:length(24)}")]
//        public async Task<IActionResult> Update(string id, Contact contactIn) {
//            var contact = await _contactService.Get(id);
//            if(contact == null) {
//                return NotFound();
//            }
//            await _contactService.Update(id, contactIn);
//            return NoContent();
//        }

//        [HttpDelete("{id:length(24)}")]
//        public async Task<IActionResult> Delete(string id) {
//            var contact = await _contactService.Get(id);
//            if(contact == null) {
//                return NotFound();
//            }
//            await _contactService.Remove(contact.Id);
//            return NoContent();
//        }
//    }
//}
