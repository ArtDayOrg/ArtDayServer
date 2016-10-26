using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ArtDayEmber;
using System.Web.Http.Cors;
using WebAPI.Models;
using System.Web;
using System.IO;

namespace ArtDayEmber.Controllers
{
    [EnableCors(origins: "http://localhost:4200", headers: "*", methods: "*")]
    public class ImageController : ApiController
    {
        private const string CONTAINER = "images";

        // POST: api/Sessions
        // Creates a new session
        [ResponseType(typeof(void))]
        public async Task<HttpResponseMessage> PostImage()
        {
            var context = new StorageContext();

            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            // Get and create the container
            var blobContainer = context.BlobClient.GetContainerReference(CONTAINER);
            blobContainer.CreateIfNotExists();

            string root = HttpContext.Current.Server.MapPath("~/App_Data");
            var provider = new MultipartFormDataStreamProvider(root);

            try
            {
                // Read the form data and return an async task.
                await Request.Content.ReadAsMultipartAsync(provider);

                // This illustrates how to get the file names for uploaded files.
                foreach (var fileData in provider.FileData)
                {
                    var filename = fileData.Headers.ContentDisposition.Name.Replace("\"", "");
                    var blob = blobContainer.GetBlockBlobReference(filename);
                    blob.Properties.ContentType = fileData.Headers.ContentType.MediaType;
                    using (var filestream = File.OpenRead(fileData.LocalFileName))
                    {
                        blob.UploadFromStream(filestream);
                    }
                    File.Delete(fileData.LocalFileName);
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (System.Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }
    }
}