using API.Classes;
using API.Models;
using API.Services;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers {
    [ApiVersion("1.0")]
    [ODataRoutePrefix("contacts")]
    public class ContactsController: ODataController {

        private readonly ContactService _contactService;
        private readonly Logger _logger;

        public ContactsController(ContactService contactService, Logger logger) {
            _contactService = contactService;
            _logger = logger;
        }

        [HttpGet]
        [ODataRoute("")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<Contact>),200)] // Ok
        [EnableQuery]
        public IActionResult Get() {
            /*
            Working = $count, $filter, $orderBy, $skip, $top
            Not working = $select, $expand ($select does work for GetById)
            Mongo Team working on fix for $select and $expand
                https://jira.mongodb.org/browse/CSHARP-1423
                https://jira.mongodb.org/browse/CSHARP-1771
                in meantime, remove them from query, then apply, then apply second LINQ re-applying select
            */
            return Ok(_contactService.Get());
        }

        [HttpGet]
        [ODataRoute("({id})")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Contact),200)] // Ok
        [ProducesResponseType(typeof(void),404)] // Not Found
        [EnableQuery]
        public async Task<IActionResult> GetById(string id) {
            // Working = $select
            // Not working = $expand
            // Not needed = $count, $filter, $orderBy, $skip, $top
            return Ok(await _contactService.Get(id));
        }

        [HttpPost]
        [ODataRoute("")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Contact),201)] // Created
        [ProducesResponseType(typeof(ModelStateDictionary),400)] // Bad Request
        [ProducesResponseType(typeof(void),401)] // Unauthorized
        [ProducesResponseType(typeof(string),409)] // Conflict
        public async Task<IActionResult> Post([FromBody] Contact contact) {
            await _contactService.Create(contact);
            string message = "{action:\"POST\", \"name\":\"Contact\", {\"value\": " + JsonConvert.SerializeObject(contact) + "}";
            _logger.Send(message);
            return Ok(contact);
        }

        [HttpPatch]
        [ODataRoute("({id})")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Contact),204)] // Updated
        [ProducesResponseType(typeof(ModelStateDictionary),400)] // Bad Request
        [ProducesResponseType(typeof(void),401)] // Unauthorized - User not authenticated
        [ProducesResponseType(typeof(void),403)] // Forbidden - User does not have required claim roles
        [ProducesResponseType(typeof(void),404)] // Not Found
        public async Task<IActionResult> Patch(string id,[FromBody] Delta<Contact> delta) {
            var contact = await _contactService.Get(id);
            if(contact == null) {
                return NotFound();
            }
            delta.Patch(contact);
            await _contactService.Update(id,contact);
            string message = "{action:\"PATCH\", \"name\":\"Contact\", {\"value\": " + JsonConvert.SerializeObject(contact) + "}";
            _logger.Send(message);
            return NoContent();
        }

        [HttpDelete]
        [ODataRoute("({id})")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(void),204)] // No Content
        [ProducesResponseType(typeof(void),401)] // Unauthorized
        [ProducesResponseType(typeof(void),404)] // Not Found
        [ProducesResponseType(typeof(string),409)] // Conflict
        public async Task<IActionResult> Delete(string id) {
            var contact = await _contactService.Get(id);
            if(contact == null) {
                return NotFound();
            }
            await _contactService.Remove(contact.Id);
            string message = "{action:\"DELETE\", \"name\":\"Contact\", {\"value\": " + id + "}";
            _logger.Send(message);
            return NoContent();
        }
    }
}