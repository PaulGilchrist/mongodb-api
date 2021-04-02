using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDbApi.Models;
using MongoDbApi.Services;

namespace MongoDbApi.Controllers {
    [ODataRoutePrefix("contacts")]
    public class ContactsController: ODataController {

        private readonly ContactService _contactService;

        public ContactsController(ContactService contactService) {
            _contactService = contactService;
        }

        [HttpGet]
        [ODataRoute("")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<Contact>),200)] // Ok
        [EnableQuery(HandleNullPropagation = HandleNullPropagationOption.False)]
        //[EnableQuery(AllowedQueryOptions=AllowedQueryOptions.Count|AllowedQueryOptions.Filter| AllowedQueryOptions.OrderBy|AllowedQueryOptions.Skip|AllowedQueryOptions.Top,HandleNullPropagation=HandleNullPropagationOption.False)]
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
            return NoContent();
        }
    }
}