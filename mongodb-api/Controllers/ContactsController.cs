using MongoDbApi.Models;
using MongoDbApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace MongoDbApi.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class ContactsController: ControllerBase {

        private readonly ContactService _contactService;


        public ContactsController(ContactService contactService) {
            _contactService = contactService;
        }

        [HttpGet]
        public ActionResult<List<Contact>> Get() {
            return _contactService.Get();
        }

        [HttpGet("{id:length(24)}",Name = "GetContact")]
        public ActionResult<Contact> Get(string id) {
            var contact = _contactService.Get(id);
            if(contact == null) {
                return NotFound();
            }
            return contact;
        }

        [HttpPost]
        public ActionResult<Contact> Create(Contact contact) {
            _contactService.Create(contact);
            return CreatedAtRoute("GetContact",new { id = contact.Id.ToString() },contact);
        }

        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id,Contact contactIn) {
            var contact = _contactService.Get(id);
            if(contact == null) {
                return NotFound();
            }
            _contactService.Update(id,contactIn);
            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id) {
            var contact = _contactService.Get(id);
            if(contact == null) {
                return NotFound();
            }
            _contactService.Remove(contact.Id);
            return NoContent();
        }
    }
}
